using Microsoft.EntityFrameworkCore;
using DateSpark.API.Data;
using DateSpark.API.Models;

namespace DateSpark.API.Services
{
    public class IdeaGeneratorService : IIdeaGeneratorService
    {
        private readonly AppDbContext _context;

        public IdeaGeneratorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Idea>> GetIdeasAsync(IdeaFilters filters)
        {
            return await GetFilteredIdeasAsync(filters);
        }

        public async Task<List<Idea>> GetFilteredIdeasAsync(IdeaFilters filters)
        {
            var query = _context.Ideas.AsQueryable();

            // Применяем фильтры
            if (!string.IsNullOrEmpty(filters.Category))
                query = query.Where(i => i.Category == filters.Category);
            
            if (!string.IsNullOrEmpty(filters.Location))
                query = query.Where(i => i.Location == filters.Location);
            
            if (!string.IsNullOrEmpty(filters.Mood))
                query = query.Where(i => i.Mood == filters.Mood);
            
            if (!string.IsNullOrEmpty(filters.Duration))
                query = query.Where(i => i.Duration == filters.Duration);
            
            if (!string.IsNullOrEmpty(filters.Weather))
                query = query.Where(i => i.Weather == filters.Weather);
            
            if (filters.PriceCategory.HasValue)
                query = query.Where(i => i.PriceCategory == filters.PriceCategory.Value);
            
            if (filters.OnlyActive)
                query = query.Where(i => i.IsActive);

            return await query.ToListAsync();
        }

        public async Task<Idea?> GetRandomIdeaAsync(IdeaFilters? filters = null)
        {
            filters ??= new IdeaFilters();
            var ideas = await GetFilteredIdeasAsync(filters);
            
            if (!ideas.Any()) 
                return null;
            
            var random = new Random();
            return ideas[random.Next(ideas.Count)];
        }

        public async Task<bool> VoteForIdeaAsync(IdeaVote vote)
        {
            try
            {
                // Проверяем, существует ли уже голос
                var existingVote = await _context.IdeaVotes
                    .FirstOrDefaultAsync(v => v.IdeaId == vote.IdeaId && v.UserId == vote.UserId);

                var idea = await _context.Ideas.FindAsync(vote.IdeaId);
                if (idea == null) return false;

                if (existingVote != null)
                {
                    // Обновляем существующий голос
                    if (existingVote.IsLike && !vote.IsLike)
                    {
                        idea.Likes--;
                        idea.Dislikes++;
                    }
                    else if (!existingVote.IsLike && vote.IsLike)
                    {
                        idea.Dislikes--;
                        idea.Likes++;
                    }

                    existingVote.IsLike = vote.IsLike;
                    existingVote.VotedAt = DateTime.UtcNow;
                }
                else
                {
                    // Добавляем новый голос
                    await _context.IdeaVotes.AddAsync(vote);
                    
                    if (vote.IsLike)
                        idea.Likes++;
                    else
                        idea.Dislikes++;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VoteForIdeaAsync: {ex.Message}");
                return false;
            }
        }
    }
}