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

            // КАТЕГОРИЯ: поддерживает несколько через запятую
            if (!string.IsNullOrEmpty(filters.Category))
            {
                var categories = filters.Category.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (categories.Length > 0)
                {
                    query = query.Where(i => categories.Contains(i.Category.Trim()));
                }
            }

            // ЛОКАЦИЯ
            if (!string.IsNullOrEmpty(filters.Location))
            {
                var locations = filters.Location.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (locations.Length > 0)
                {
                    query = query.Where(i => locations.Contains(i.Location.Trim()));
                }
            }

            // НАСТРОЕНИЕ
            if (!string.IsNullOrEmpty(filters.Mood))
            {
                var moods = filters.Mood.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (moods.Length > 0)
                {
                    query = query.Where(i => moods.Contains(i.Mood.Trim()));
                }
            }

            // ПОГОДА - теперь поддерживает "Солнечно,Снег"
            if (!string.IsNullOrEmpty(filters.Weather))
            {
                // Если "Любая" - не фильтруем
                if (filters.Weather != "Любая")
                {
                    var weatherOptions = filters.Weather
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => w.Trim())
                        .ToList();

                    if (weatherOptions.Count > 0)
                    {
                        query = query.Where(i => weatherOptions.Contains(i.Weather));
                    }
                }
            }

            if (filters.PriceCategory.HasValue)
            {
                query = query.Where(i => i.PriceCategory == filters.PriceCategory.Value);
            }

            if (filters.OnlyActive)
            {
                query = query.Where(i => i.IsActive);
            }

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

        // УПРОЩАЕМ ГОЛОСОВАНИЕ - РАБОТАЕМ С POЛЯМИ В IDEAS
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

                // ПРОСТО ОБНОВЛЯЕМ СЧЕТЧИКИ В ТАБЛИЦЕ IDEAS
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