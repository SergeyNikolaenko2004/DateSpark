using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DateSpark.API.Data;
using DateSpark.API.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DateSpark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ProfileResponse>> GetProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                var user = await _context.Users
                    .Include(u => u.UserCouples)
                    .ThenInclude(uc => uc.Couple)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { Success = false, Message = "Пользователь не найден" });
                }

                var userCouple = user.UserCouples.FirstOrDefault();
                CoupleInfo? coupleInfo = null;
                List<PartnerInfo> partners = new List<PartnerInfo>();

                if (userCouple?.Couple != null)
                {
                    coupleInfo = new CoupleInfo
                    {
                        Id = userCouple.Couple.Id,
                        Name = userCouple.Couple.Name,
                        JoinCode = userCouple.Couple.JoinCode,
                        CreatedAt = userCouple.Couple.CreatedAt
                    };

                    // Получаем всех участников пары
                    var allPartners = await _context.UserCouples
                        .Where(uc => uc.CoupleId == userCouple.CoupleId)
                        .Include(uc => uc.User)
                        .ToListAsync();

                    partners = allPartners.Select(uc => new PartnerInfo
                    {
                        Id = uc.User.Id,
                        Name = uc.User.Name,
                        Email = uc.User.Email,
                        Role = uc.Role,
                        JoinedAt = uc.JoinedAt
                    }).ToList();
                }

                return Ok(new ProfileResponse
                {
                    Success = true,
                    User = new UserInfo 
                    { 
                        Id = user.Id, 
                        Email = user.Email, 
                        Name = user.Name,
                        Avatar = user.Avatar,
                        CreatedAt = user.CreatedAt
                    },
                    Couple = coupleInfo,
                    Partners = partners
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Ошибка при получении профиля" });
            }
        }

        [HttpPut("user")]
        public async Task<ActionResult<AuthResponse>> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new AuthResponse { Success = false, Message = "Пользователь не найден" });
                }

                if (!string.IsNullOrEmpty(request.Name))
                {
                    user.Name = request.Name;
                }

                if (!string.IsNullOrEmpty(request.Avatar))
                {
                    user.Avatar = request.Avatar;
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new AuthResponse 
                { 
                    Success = true, 
                    Message = "Профиль обновлен",
                    User = new UserDto { Id = user.Id, Email = user.Email, Name = user.Name, Avatar = user.Avatar }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse { Success = false, Message = "Ошибка при обновлении профиля" });
            }
        }
    }

    public class ProfileResponse
    {
        public bool Success { get; set; }
        public UserInfo User { get; set; } = new UserInfo();
        public CoupleInfo? Couple { get; set; }
        public List<PartnerInfo> Partners { get; set; } = new List<PartnerInfo>();
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CoupleInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PartnerInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Avatar { get; set; }
    }
}