using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DateSpark.API.Models
{
    public class Idea
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        // Enum для базы данных
        public PriceCategory PriceCategory { get; set; } = PriceCategory.Medium;
        
        // Свойство для API (возвращает символ)
        [JsonIgnore]
        public string PriceSymbol => PriceCategory.ToSymbol();
        
        // Остальные поля
        public string Location { get; set; } = "Любая";
        public string Mood { get; set; } = "Любое"; 
        public string Duration { get; set; } = "Любое";
        public string Weather { get; set; } = "Любая";
        
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}