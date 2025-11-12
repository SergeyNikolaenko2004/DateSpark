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
                // 🔥 ДОБАВЬ ЛОГИРОВАНИЕ
                Console.WriteLine($"Saving vote: UserId={vote.UserId}, IdeaId={vote.IdeaId}, IsLike={vote.IsLike}");
                
                _context.IdeaVotes.Add(vote);
                await _context.SaveChangesAsync();
                
                Console.WriteLine("✅ Vote saved to database");
                return true;
            }
            catch (Exception ex)
            {
                // 🔥 ЛОГИРУЙ ОШИБКИ БАЗЫ ДАННЫХ
                Console.WriteLine($"❌ Database error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
}