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
        public async Task<ActionResult> VoteForIdea([FromBody] VoteRequest voteRequest)
        {
            try
            {
                // 🔥 ДОБАВЬ ЛОГИРОВАНИЕ
                Console.WriteLine($"Received vote: IdeaId={voteRequest.IdeaId}, IsLike={voteRequest.IsLike}");
                
                // Извлекаем userId из JWT токена
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                Console.WriteLine($"User claims: {User.Claims.Count()}");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }
                
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    Console.WriteLine("❌ User ID not found in token");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                Console.WriteLine($"✅ User ID from token: {userId}");
                
                // Создаем IdeaVote с userId из токена
                var vote = new IdeaVote
                {
                    IdeaId = voteRequest.IdeaId,
                    UserId = userId,
                    IsLike = voteRequest.IsLike,
                    VotedAt = DateTime.UtcNow
                };
                
                var result = await _ideaService.VoteForIdeaAsync(vote);
                if (!result) 
                {
                    Console.WriteLine("❌ Failed to record vote in service");
                    return BadRequest(new { message = "Failed to record vote" });
                }
                
                Console.WriteLine("✅ Vote recorded successfully");
                return Ok(new { message = "Vote recorded successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in Vote: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
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