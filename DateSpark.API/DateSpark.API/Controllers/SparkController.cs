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
        [Authorize] // 🔥 АУТЕНТИФИКАЦИЯ ВСЕ ЕЩЕ НУЖНА ДЛЯ ТРЕКИНГА
        public async Task<ActionResult> VoteForIdea([FromBody] VoteRequest voteRequest)
        {
            try
            {
                // Извлекаем userId из JWT токена для логов (но не используем в логике)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = userIdClaim?.Value ?? "unknown";

                Console.WriteLine($"✅ VOTE: User {userId}, Idea {voteRequest.IdeaId}, Like {voteRequest.IsLike}");

                // 🔥 ПРОСТО ВЫЗЫВАЕМ ОБНОВЛЕНИЕ СЧЕТЧИКОВ - UserId не нужен
                var result = await _ideaService.VoteForIdeaAsync(voteRequest.IdeaId, voteRequest.IsLike);

                if (!result)
                    return BadRequest(new { message = "Failed to record vote" });

                return Ok(new { message = "Vote recorded successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in Vote: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
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