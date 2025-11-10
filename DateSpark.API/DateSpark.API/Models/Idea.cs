using System.ComponentModel.DataAnnotations;

namespace DateSpark.API.Models
{
    public class Idea
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        
        // НОВЫЕ ПОЛЯ ДЛЯ ГЕНЕРАТОРА:
        public string Location { get; set; } = "Любая"; // Дома, Город, Природа
        public string Mood { get; set; } = "Любое"; // Романтическое, Активное, Уютное
        public string Duration { get; set; } = "Любое"; // Короткое (1-2ч), Вечер (3-4ч), Целый день
        public string Weather { get; set; } = "Любая"; // Любая, Только ясно, Только дома
        
        // СИСТЕМА ЛАЙКОВ:
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}