using System.ComponentModel.DataAnnotations;


namespace DateSpark.API.Models
{
    public class Couple
    {
        public int Id { get; set; }

        public string Name { get; set; } = "Наша пара";

        [Required]
        public string JoinCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public ICollection<UserCouple> UserCouples { get; set; } = new List<UserCouple>();
    }
}