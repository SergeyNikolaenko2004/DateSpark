using DateSpark.API.Models;
using DateSpark.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DateSpark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SparkController : ControllerBase
    {
        private readonly IIdeaGeneratorService _ideaService;

        public SparkController(IIdeaGeneratorService ideaService)
        {
            _ideaService = ideaService;
        }

        [HttpGet("random")]
        [AllowAnonymous]
        public async Task<ActionResult<Idea>> GetRandomIdea([FromQuery] IdeaFilters filters)
        {
            var idea = await _ideaService.GetRandomIdeaAsync(filters);
            if (idea == null) return NotFound("No ideas found");
            return Ok(idea);
        }

        [HttpPost("vote")]
        [Authorize]
        public async Task<ActionResult> VoteForIdea([FromBody] IdeaVote vote)
        {
            try
            {
                // 🔥 ИЗВЛЕКАЕМ userId ИЗ JWT ТОКЕНА
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // 🔥 УСТАНАВЛИВАЕМ userId ДЛЯ ГОЛОСА
                vote.UserId = userId;
                vote.VotedAt = DateTime.UtcNow; // 🔥 УСТАНАВЛИВАЕМ ВРЕМЯ
                
                var result = await _ideaService.VoteForIdeaAsync(vote);
                if (!result) return BadRequest(new { message = "Failed to record vote" });
                
                return Ok(new { message = "Vote recorded successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("filtered")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Idea>>> GetFilteredIdeas([FromQuery] IdeaFilters filters)
        {
            var ideas = await _ideaService.GetFilteredIdeasAsync(filters);
            return Ok(ideas);
        }
    }
}