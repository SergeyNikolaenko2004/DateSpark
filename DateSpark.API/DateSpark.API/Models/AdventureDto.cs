namespace DateSpark.API.Models
{
    // 🔥 DTO для создания карточки из идеи
    public class CreateAdventureFromIdeaRequest
    {
        public int IdeaId { get; set; }
    }

    // 🔥 DTO для ручного создания карточки
    public class CreateAdventureManualRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // 🔥 DTO для обновления статуса
    public class UpdateAdventureStatusRequest
    {
        public AdventureStatus Status { get; set; }
    }

    // 🔥 DTO для обновления даты
    public class UpdateAdventureDateRequest
    {
        public DateTime? PlannedDate { get; set; }
    }

    // 🔥 DTO для обновления заметок
    public class UpdateAdventureNotesRequest
    {
        public string Notes { get; set; } = string.Empty;
    }

    // 🔥 DTO для завершения приключения
    public class CompleteAdventureRequest
    {
        public string PhotoUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    // 🔥 DTO для ответа (клиенту)
    public class AdventureResponse
    {
        public int Id { get; set; }
        public int? IdeaId { get; set; }
        public int CoupleId { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AdventureStatus Status { get; set; }
        public string StatusSymbol { get; set; } = string.Empty;
        public DateTime? PlannedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}