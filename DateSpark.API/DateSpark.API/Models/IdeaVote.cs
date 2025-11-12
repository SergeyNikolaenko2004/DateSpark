namespace DateSpark.API.Models
{
    public class IdeaVote
    {
        public int Id { get; set; }
        public int IdeaId { get; set; }
        public int UserId { get; set; }
        public bool IsLike { get; set; } // true = ❤️, false = 💔
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public Idea Idea { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}