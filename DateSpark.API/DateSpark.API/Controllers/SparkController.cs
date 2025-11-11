using DateSpark.API.Models;
using DateSpark.API.Services;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<Idea>> GetRandomIdea([FromQuery] IdeaFilters filters)
        {
            var idea = await _ideaService.GetRandomIdeaAsync(filters);
            if (idea == null) return NotFound("No ideas found");
            return Ok(idea);
        }

        [HttpPost("vote")]
        public async Task<ActionResult> VoteForIdea([FromBody] IdeaVote vote)
        {
            var result = await _ideaService.VoteForIdeaAsync(vote);
            if (!result) return NotFound("Idea not found");
            return Ok(new { message = "Vote recorded" });
        }

        [HttpGet("filtered")]
        public async Task<ActionResult<List<Idea>>> GetFilteredIdeas([FromQuery] IdeaFilters filters)
        {
            var ideas = await _ideaService.GetFilteredIdeasAsync(filters);
            return Ok(ideas);
        }
    }
}