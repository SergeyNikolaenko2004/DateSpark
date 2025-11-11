using System.ComponentModel.DataAnnotations;


namespace DateSpark.API.Models
{
    public class UserCouple
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public int CoupleId { get; set; }
        public Couple Couple { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "member"; // "creator" или "member"

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}