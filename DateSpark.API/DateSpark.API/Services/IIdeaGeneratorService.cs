using DateSpark.API.Models;

namespace DateSpark.API.Services
{
    public interface IIdeaGeneratorService
    {
        Task<List<Idea>> GetIdeasAsync(IdeaFilters filters);
        Task<Idea?> GetRandomIdeaAsync(IdeaFilters? filters = null);
        Task<List<Idea>> GetFilteredIdeasAsync(IdeaFilters filters);
        Task<bool> VoteForIdeaAsync(int ideaId, bool isLike);
    }
}