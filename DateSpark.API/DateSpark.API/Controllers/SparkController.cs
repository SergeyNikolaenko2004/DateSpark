using Microsoft.AspNetCore.Mvc;
using DateSpark.API.Models;
using DateSpark.API.Services;

namespace DateSpark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SparkController : ControllerBase
    {
        private readonly IIdeaGeneratorService _ideaGeneratorService;

        public SparkController(IIdeaGeneratorService ideaGeneratorService)
        {
            _ideaGeneratorService = ideaGeneratorService;
        }

        // GET: api/spark/random
        [HttpGet("random")]
        public async Task<ActionResult<Idea>> GetRandomIdea([FromQuery] IdeaFilters filters)
        {
            try
            {
                var idea = await _ideaGeneratorService.GetRandomIdeaAsync(filters);
                
                if (idea == null)
                {
                    return NotFound("No ideas found matching your filters");
                }

                return idea;
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"❌ Error in GetRandomIdea: {ex.Message}");
                return StatusCode(500, new { 
                    error = "Internal server error", 
                    message = ex.Message 
                });
            }
        }

        // GET: api/spark/filtered
        [HttpGet("filtered")]
        public async Task<ActionResult<List<Idea>>> GetFilteredIdeas([FromQuery] IdeaFilters filters)
        {
            try
            {
                var ideas = await _ideaGeneratorService.GetFilteredIdeasAsync(filters);
                return ideas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetFilteredIdeas: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // POST: api/spark/vote
        [HttpPost("vote")]
        public async Task<ActionResult> VoteForIdea([FromBody] IdeaVote vote)
        {
            try
            {
                var success = await _ideaGeneratorService.VoteForIdeaAsync(vote);
                
                if (!success)
                {
                    return NotFound("Idea not found");
                }

                return Ok(new { message = "Vote recorded successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in VoteForIdea: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // GET: api/spark/categories
        [HttpGet("categories")]
        public ActionResult<object> GetAvailableCategories()
        {
            return new
            {
                Locations = new[] { "Любая", "Дома", "Город", "Природа" },
                Moods = new[] { "Любое", "Романтическое", "Активное", "Уютное", "Творческое" },
                Durations = new[] { "Любое", "Короткое (1-2ч)", "Вечер (3-4ч)", "Целый день" },
                Weather = new[] { "Любая", "Ясно", "Дождь", "Только дома" },
                Categories = new[] { "Любая", "Домашнее", "Активный отдых", "Развлечения", "Романтическое" }
            };
        }

        // GET: api/spark/test
        [HttpGet("test")]
        public ActionResult Test()
        {
            return Ok(new { message = "SparkController is working!", status = "OK" });
        }
    }
}