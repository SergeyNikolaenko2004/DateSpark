using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DateSpark.API.Models
{
    // Статусы карточки в канбане
    public enum AdventureStatus
    {
        Liked,      // Лайкнуто (показано в Spark)
        Planned,    // Запланировано (выбрали дату)
        InProgress, // В процессе (сегодня делаем)
        Completed   // Выполнено (ура!)
    }

    public class AdventureCard
    {
        public int Id { get; set; }
        
        // Связь с оригинальной идеей (может быть null)
        public int? IdeaId { get; set; }
        
        // К какой паре принадлежит (обязательно)
        [Required]
        public int CoupleId { get; set; }
        
        // Кто создал карточку (или лайкнул идею)
        [Required]
        public int CreatedByUserId { get; set; }
        
        // Название (если нет IdeaId - пользователь ввел сам)
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
        // Описание
        public string Description { get; set; } = string.Empty;
        
        // Текущий статус в канбане
        [Required]
        public AdventureStatus Status { get; set; } = AdventureStatus.Liked;
        
        // Когда планируем сделать (для статуса Planned/InProgress)
        public DateTime? PlannedDate { get; set; }
        
        // Когда сделали (для статуса Completed)
        public DateTime? CompletedDate { get; set; }
        
        // Заметки от пары (можно оставить пожелания)
        public string Notes { get; set; } = string.Empty;
        
        // Фото после выполнения (можно прикрепить потом)
        public string PhotoUrl { get; set; } = string.Empty;
        
        // Когда создали
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Когда последний раз обновляли
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства (для Entity Framework)
        [JsonIgnore]
        public Idea? Idea { get; set; }
        
        [JsonIgnore]
        public Couple? Couple { get; set; }
        
        [JsonIgnore]
        public User? CreatedByUser { get; set; }
        
        // 💡 Удобное свойство для фронтенда - символ статуса
        public string StatusSymbol => Status switch
        {
            AdventureStatus.Liked => "💡",
            AdventureStatus.Planned => "📅",
            AdventureStatus.InProgress => "🚀",
            AdventureStatus.Completed => "✅",
            _ => "📌"
        };
    }
}