using Microsoft.AspNetCore.Mvc;
using DateSpark.API.Models;

namespace DateSpark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdeasController : ControllerBase
    {
        private static List<Idea> _ideas = new()
        {
            new Idea { Id = 1, Title = "Романтический ужин", Description = "Приготовить ужин при свечах", Category = "Домашнее", Price = 30 },
            new Idea { Id = 2, Title = "Прогулка в парке", Description = "Прогулка и пикник", Category = "Активное", Price = 15 }
        };

        // GET: api/ideas
        [HttpGet]
        public ActionResult<IEnumerable<Idea>> GetIdeas()
        {
            return Ok(_ideas);
        }

        // GET: api/ideas/5
        [HttpGet("{id}")]
        public ActionResult<Idea> GetIdea(int id)
        {
            var idea = _ideas.FirstOrDefault(i => i.Id == id);
            if (idea == null) return NotFound();
            return idea;
        }

        // POST: api/ideas
        [HttpPost]
        public ActionResult<Idea> PostIdea(Idea idea)
        {
            idea.Id = _ideas.Count + 1;
            _ideas.Add(idea);
            return CreatedAtAction(nameof(GetIdea), new { id = idea.Id }, idea);
        }
    }
}