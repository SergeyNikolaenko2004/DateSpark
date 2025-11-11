using DateSpark.API.Data;
using DateSpark.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DateSpark.API.Services
{
    public class IdeaGeneratorService : IIdeaGeneratorService
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new Random();

        public IdeaGeneratorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Idea?> GetRandomIdeaAsync(IdeaFilters? filters = null)
        {
            var query = _context.Ideas.AsQueryable();

            if (filters != null)
            {
                query = ApplyFilters(query, filters);
            }

            query = query.Where(i => i.IsActive);

            var count = await query.CountAsync();
            if (count == 0) return null;

            var skip = _random.Next(0, count);
            var idea = await query.Skip(skip).FirstOrDefaultAsync();

            return idea;
        }

        public async Task<List<Idea>> GetFilteredIdeasAsync(IdeaFilters filters)
        {
            var query = _context.Ideas.AsQueryable();
            query = ApplyFilters(query, filters);
            return await query.Where(i => i.IsActive).ToListAsync();
        }

        public async Task<bool> VoteForIdeaAsync(IdeaVote vote)
        {
            var idea = await _context.Ideas.FindAsync(vote.IdeaId);
            if (idea == null) return false;

            if (vote.IsLike)
                idea.Likes++;
            else
                idea.Dislikes++;

            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<Idea> ApplyFilters(IQueryable<Idea> query, IdeaFilters filters)
        {
            if (!string.IsNullOrEmpty(filters.Category) && filters.Category != "Любое")
                query = query.Where(i => i.Category == filters.Category || i.Category == "Любое");

            if (!string.IsNullOrEmpty(filters.Location) && filters.Location != "Любая")
                query = query.Where(i => i.Location == filters.Location || i.Location == "Любая");

            if (!string.IsNullOrEmpty(filters.Mood) && filters.Mood != "Любое")
                query = query.Where(i => i.Mood == filters.Mood || i.Mood == "Любое");

            if (!string.IsNullOrEmpty(filters.Duration) && filters.Duration != "Любое")
                query = query.Where(i => i.Duration == filters.Duration || i.Duration == "Любое");

            if (!string.IsNullOrEmpty(filters.Weather) && filters.Weather != "Любая")
                query = query.Where(i => i.Weather == filters.Weather || i.Weather == "Любая");

            if (filters.MaxPrice.HasValue)
                query = query.Where(i => i.Price <= filters.MaxPrice.Value);

            if (filters.OnlyActive)
                query = query.Where(i => i.IsActive);

            return query;
        }
    }
}