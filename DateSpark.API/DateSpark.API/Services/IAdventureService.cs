using DateSpark.API.Models;

namespace DateSpark.API.Services
{
    public interface IAdventureService
    {
        // 🔥 Создать карточку из лайкнутой идеи
        Task<AdventureCard> CreateFromIdeaAsync(int ideaId, int coupleId, int userId);
        
        // 🔥 Создать карточку вручную
        Task<AdventureCard> CreateManualAsync(string title, string description, int coupleId, int userId);
        
        // 🔥 Получить все карточки пары
        Task<List<AdventureCard>> GetCoupleAdventuresAsync(int coupleId);
        
        // 🔥 Получить карточки пары по статусу
        Task<List<AdventureCard>> GetCoupleAdventuresByStatusAsync(int coupleId, AdventureStatus status);
        
        // 🔥 Обновить статус карточки
        Task<AdventureCard> UpdateStatusAsync(int adventureId, AdventureStatus newStatus, int userId);
        
        // 🔥 Обновить дату планирования
        Task<AdventureCard> UpdatePlannedDateAsync(int adventureId, DateTime? plannedDate, int userId);
        
        // 🔥 Обновить заметки
        Task<AdventureCard> UpdateNotesAsync(int adventureId, string notes, int userId);
        
        // 🔥 Отметить как выполненное
        Task<AdventureCard> CompleteAdventureAsync(int adventureId, string photoUrl, string notes, int userId);
        
        // 🔥 Удалить карточку
        Task<bool> DeleteAdventureAsync(int adventureId, int userId);
        
        // 🔥 Проверить, можно ли создать из идеи (чтобы не дублировать)
        Task<bool> CanCreateFromIdeaAsync(int ideaId, int coupleId);
    }
}