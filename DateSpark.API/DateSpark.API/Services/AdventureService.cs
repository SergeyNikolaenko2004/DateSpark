using Microsoft.EntityFrameworkCore;
using DateSpark.API.Data;
using DateSpark.API.Models;

namespace DateSpark.API.Services
{
    public class AdventureService : IAdventureService
    {
        private readonly AppDbContext _context;

        public AdventureService(AppDbContext context)
        {
            _context = context;
        }

        // Создать карточку из лайкнутой идеи
        public async Task<AdventureCard> CreateFromIdeaAsync(int ideaId, int coupleId, int userId)
        {
            // Проверяем, что идея существует
            var idea = await _context.Ideas.FindAsync(ideaId);
            if (idea == null)
                throw new ArgumentException("Идея не найдена");

            // Проверяем, что пара существует
            var couple = await _context.Couples.FindAsync(coupleId);
            if (couple == null)
                throw new ArgumentException("Пара не найдена");

            // Проверяем, что пользователь состоит в этой паре
            var userInCouple = await _context.UserCouples
                .AnyAsync(uc => uc.UserId == userId && uc.CoupleId == coupleId);

            if (!userInCouple)
                throw new UnauthorizedAccessException("Вы не состоите в этой паре");

            // Проверяем, нет ли уже такой карточки
            var existingCard = await _context.AdventureCards
                .FirstOrDefaultAsync(ac => ac.IdeaId == ideaId && ac.CoupleId == coupleId);

            if (existingCard != null)
                throw new InvalidOperationException("Эта идея уже добавлена в вашу доску");

            // Создаем новую карточку
            var adventureCard = new AdventureCard
            {
                IdeaId = ideaId,
                CoupleId = coupleId,
                CreatedByUserId = userId,
                Title = idea.Title,
                Description = idea.Description,
                Status = AdventureStatus.Liked, // Начинаем с "Лайкнуто"
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AdventureCards.Add(adventureCard);
            await _context.SaveChangesAsync();

            return adventureCard;
        }

        // Создать карточку вручную
        public async Task<AdventureCard> CreateManualAsync(string title, string description, int coupleId, int userId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название обязательно");

            // Проверяем, что пара существует
            var couple = await _context.Couples.FindAsync(coupleId);
            if (couple == null)
                throw new ArgumentException("Пара не найдена");

            // Проверяем, что пользователь состоит в этой паре
            var userInCouple = await _context.UserCouples
                .AnyAsync(uc => uc.UserId == userId && uc.CoupleId == coupleId);

            if (!userInCouple)
                throw new UnauthorizedAccessException("Вы не состоите в этой паре");

            // Создаем карточку
            var adventureCard = new AdventureCard
            {
                IdeaId = null, // Ручное создание
                CoupleId = coupleId,
                CreatedByUserId = userId,
                Title = title.Trim(),
                Description = description?.Trim() ?? "",
                Status = AdventureStatus.Liked,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AdventureCards.Add(adventureCard);
            await _context.SaveChangesAsync();

            return adventureCard;
        }

        // Получить все карточки пары
        public async Task<List<AdventureCard>> GetCoupleAdventuresAsync(int coupleId)
        {
            return await _context.AdventureCards
                .Where(ac => ac.CoupleId == coupleId)
                .OrderByDescending(ac => ac.CreatedAt)
                .ToListAsync();
        }

        // Получить карточки пары по статусу
        public async Task<List<AdventureCard>> GetCoupleAdventuresByStatusAsync(int coupleId, AdventureStatus status)
        {
            return await _context.AdventureCards
                .Where(ac => ac.CoupleId == coupleId && ac.Status == status)
                .OrderBy(ac => ac.PlannedDate ?? DateTime.MaxValue) // Сначала запланированные
                .ThenByDescending(ac => ac.CreatedAt)
                .ToListAsync();
        }

        // Обновить статус карточки
        public async Task<AdventureCard> UpdateStatusAsync(int adventureId, AdventureStatus newStatus, int userId)
        {
            var adventureCard = await _context.AdventureCards
                .Include(ac => ac.Couple)
                .FirstOrDefaultAsync(ac => ac.Id == adventureId);

            if (adventureCard == null)
                throw new ArgumentException("Карточка не найдена");

            // Проверяем, что пользователь состоит в этой паре
            var userInCouple = await _context.UserCouples
                .AnyAsync(uc => uc.UserId == userId && uc.CoupleId == adventureCard.CoupleId);

            if (!userInCouple)
                throw new UnauthorizedAccessException("Вы не состоите в этой паре");

            // Обновляем статус
            adventureCard.Status = newStatus;
            adventureCard.UpdatedAt = DateTime.UtcNow;

            // Если отмечаем как выполненное - ставим дату выполнения
            if (newStatus == AdventureStatus.Completed)
            {
                adventureCard.CompletedDate = DateTime.UtcNow;
            }
            // Если перемещаем из "Выполненных" обратно - очищаем дату
            else if (adventureCard.Status == AdventureStatus.Completed && newStatus != AdventureStatus.Completed)
            {
                adventureCard.CompletedDate = null;
            }

            await _context.SaveChangesAsync();
            return adventureCard;
        }

        // Обновить дату планирования
        public async Task<AdventureCard> UpdatePlannedDateAsync(int adventureId, DateTime? plannedDate, int userId)
        {
            var adventureCard = await _context.AdventureCards
                .Include(ac => ac.Couple)
                .FirstOrDefaultAsync(ac => ac.Id == adventureId);

            if (adventureCard == null)
                throw new ArgumentException("Карточка не найдена");

            // Проверяем, что пользователь состоит в этой паре
            var userInCouple = await _context.UserCouples
                .AnyAsync(uc => uc.UserId == userId && uc.CoupleId == adventureCard.CoupleId);

            if (!userInCouple)
                throw new UnauthorizedAccessException("Вы не состоите в этой паре");

            adventureCard.PlannedDate = plannedDate;
            adventureCard.UpdatedAt = DateTime.UtcNow;

            // Если указали дату - меняем статус на "Запланировано"
            if (plannedDate.HasValue && adventureCard.Status == AdventureStatus.Liked)
            {
                adventureCard.Status = AdventureStatus.Planned;
            }
            // Если убрали дату - возвращаем в "Лайкнуто"
            else if (!plannedDate.HasValue && adventureCard.Status == AdventureStatus.Planned)
            {
                adventureCard.Status = AdventureStatus.Liked;
            }

            await _context.SaveChangesAsync();
            return adventureCard;
        }

        // Обновить заметки
        public async Task<AdventureCard> UpdateNotesAsync(int adventureId, string notes, int userId)
        {
            var adventureCard = await _context.AdventureCards
                .FirstOrDefaultAsync(ac => ac.Id == adventureId);

            if (adventureCard == null)
                throw new ArgumentException("Карточка не найдена");

            // Проверяем, что пользователь состоит в этой паре
            var userInCouple = await _context.UserCouples
                .AnyAsync(uc => uc.UserId == userId && uc.CoupleId == adventureCard.CoupleId);

            if (!userInCouple)
                throw new UnauthorizedAccessException("Вы не состоите в этой паре");

            adventureCard.Notes = notes ?? "";
            adventureCard.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return adventureCard;
        }

        // Отметить как выполненное
        public async Task<AdventureCard> CompleteAdventureAsync(int adventureId, string photoUrl, string notes, int userId)
        {
            var adventureCard = await UpdateStatusAsync(adventureId, AdventureStatus.Completed, userId);

            if (!string.IsNullOrEmpty(photoUrl))
            {
                adventureCard.PhotoUrl = photoUrl;
            }

            if (!string.IsNullOrEmpty(notes))
            {
                adventureCard.Notes = notes;
            }

            adventureCard.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return adventureCard;
        }

        // Удалить карточку
        public async Task<bool> DeleteAdventureAsync(int adventureId, int userId)
        {
            var adventureCard = await _context.AdventureCards
                .FirstOrDefaultAsync(ac => ac.Id == adventureId);

            if (adventureCard == null)
                return false;

            // Проверяем, что пользователь состоит в этой паре
            var userInCouple = await _context.UserCouples
                .AnyAsync(uc => uc.UserId == userId && uc.CoupleId == adventureCard.CoupleId);

            if (!userInCouple)
                throw new UnauthorizedAccessException("Вы не состоите в этой паре");

            _context.AdventureCards.Remove(adventureCard);
            await _context.SaveChangesAsync();

            return true;
        }

        // Проверить, можно ли создать из идеи
        public async Task<bool> CanCreateFromIdeaAsync(int ideaId, int coupleId)
        {
            var exists = await _context.AdventureCards
                .AnyAsync(ac => ac.IdeaId == ideaId && ac.CoupleId == coupleId);

            return !exists; // Можно создать если еще не существует
        }
    }
}