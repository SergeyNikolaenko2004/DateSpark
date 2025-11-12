using Microsoft.AspNetCore.Mvc;
using DateSpark.API.Models;
using DateSpark.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DateSpark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] AuthRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
        {
            var result = await _authService.LoginAsync(request);
            
            if (!result.Success)
                return Unauthorized(result);
                
            return Ok(result);
        }

        [HttpPost("create-couple")]
        [Authorize] 
        public async Task<ActionResult<AuthResponse>> CreateCouple()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new AuthResponse { Success = false, Message = "User not authenticated" });
            }

            var result = await _authService.CreateCoupleAsync(userId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("join-couple")]
        [Authorize] 
        public async Task<ActionResult<AuthResponse>> JoinCouple([FromBody] JoinCoupleRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new AuthResponse { Success = false, Message = "User not authenticated" });
            }

            var result = await _authService.JoinCoupleAsync(userId, request.JoinCode);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }
    }

    public class JoinCoupleRequest
    {
        public string JoinCode { get; set; } = string.Empty;
    }
}