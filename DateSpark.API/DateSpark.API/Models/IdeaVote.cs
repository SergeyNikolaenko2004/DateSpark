namespace DateSpark.API.Models
{
    public class IdeaVote
    {
        public int Id { get; set; }
        public int IdeaId { get; set; }
        public int UserId { get; set; } // 🔥 НЕ ДОЛЖНО БЫТЬ [Required]
        public bool IsLike { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства - тоже не должны быть обязательными
        public Idea Idea { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}