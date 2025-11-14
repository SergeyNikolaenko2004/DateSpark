using DateSpark.API.Models;
using DateSpark.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DateSpark.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(AuthRequest request);
        Task<AuthResponse> LoginAsync(AuthRequest request);
        Task<AuthResponse> CreateCoupleAsync(int userId);
        Task<AuthResponse> JoinCoupleAsync(int userId, string joinCode);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

    public async Task<AuthResponse> RegisterAsync(AuthRequest request)
    {
        // Проверяем существует ли пользователь
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new AuthResponse { Success = false, Message = "Пользователь с таким email уже существует" };
        }

        // Создаем пользователя
        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Генерируем токен
        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Регистрация успешна",
            Token = token,
            User = new UserDto { Id = user.Id, Email = user.Email, Name = user.Name }
        };
    }

    public async Task<AuthResponse> LoginAsync(AuthRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserCouples)
            .ThenInclude(uc => uc.Couple)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new AuthResponse { Success = false, Message = "Неверный email или пароль" };
        }

        // Получаем пару пользователя (может быть null)
        var userCouple = user.UserCouples.FirstOrDefault();
        CoupleDto? coupleDto = null;

        if (userCouple?.Couple != null)
        {
            coupleDto = new CoupleDto 
            { 
                Id = userCouple.Couple.Id, 
                Name = userCouple.Couple.Name, 
                JoinCode = userCouple.Couple.JoinCode 
            };
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Вход выполнен успешно",
            Token = token,
            User = new UserDto { Id = user.Id, Email = user.Email, Name = user.Name },
            Couple = coupleDto 
        };
    }

        public async Task<AuthResponse> CreateCoupleAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new AuthResponse { Success = false, Message = "Пользователь не найден" };
            }

            var couple = new Couple
            {
                Name = $"{user.Name}'s Couple",
                JoinCode = GenerateJoinCode(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Couples.Add(couple);
            await _context.SaveChangesAsync();

            var userCouple = new UserCouple
            {
                UserId = userId,
                CoupleId = couple.Id,
                Role = "creator",
                JoinedAt = DateTime.UtcNow
            };

            _context.UserCouples.Add(userCouple);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Пара создана успешно",
                Couple = new CoupleDto { Id = couple.Id, Name = couple.Name, JoinCode = couple.JoinCode }
            };
        }

        public async Task<AuthResponse> JoinCoupleAsync(int userId, string joinCode)
        {
            var user = await _context.Users.FindAsync(userId);
            var couple = await _context.Couples.FirstOrDefaultAsync(c => c.JoinCode == joinCode);

            if (user == null || couple == null)
            {
                return new AuthResponse { Success = false, Message = "Неверный код приглашения" };
            }

            // Проверяем не состоит ли пользователь уже в паре
            var existingUserCouple = await _context.UserCouples
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CoupleId == couple.Id);

            if (existingUserCouple != null)
            {
                return new AuthResponse { Success = false, Message = "Вы уже состоите в этой паре" };
            }

            var userCouple = new UserCouple
            {
                UserId = userId,
                CoupleId = couple.Id,
                Role = "member",
                JoinedAt = DateTime.UtcNow
            };

            _context.UserCouples.Add(userCouple);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Вы успешно присоединились к паре",
                Couple = new CoupleDto { Id = couple.Id, Name = couple.Name, JoinCode = couple.JoinCode }
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "your-super-secret-key-at-least-32-chars-long!"));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "DateSpark",
                audience: _configuration["Jwt:Audience"] ?? "DateSparkUsers",
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateJoinCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}