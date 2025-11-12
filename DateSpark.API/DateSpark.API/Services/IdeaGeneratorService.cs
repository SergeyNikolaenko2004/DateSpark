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

        // 🔥 УПРОЩАЕМ ГОЛОСОВАНИЕ - РАБОТАЕМ С POЛЯМИ В IDEAS
        public async Task<bool> VoteForIdeaAsync(int ideaId, bool isLike)
        {
            try
            {
                Console.WriteLine($"=== SIMPLE VOTE === Idea: {ideaId}, Like: {isLike}");
                
                // Находим идею
                var idea = await _context.Ideas.FindAsync(ideaId);
                if (idea == null)
                {
                    Console.WriteLine($"❌ Idea {ideaId} not found");
                    return false;
                }

                // 🔥 ПРОСТО ОБНОВЛЯЕМ СЧЕТЧИКИ В ТАБЛИЦЕ IDEAS
                if (isLike)
                {
                    idea.Likes++;
                    Console.WriteLine($"✅ Incremented likes for idea {ideaId}: {idea.Likes}");
                }
                else
                {
                    idea.Dislikes++;
                    Console.WriteLine($"✅ Incremented dislikes for idea {ideaId}: {idea.Dislikes}");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Vote saved successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving vote: {ex.Message}");
                return false;
            }
        }
    }
}