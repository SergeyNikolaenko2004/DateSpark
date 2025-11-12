namespace DateSpark.API.Models
{
    public class IdeaFilters
    {
        public string? Category { get; set; }
        public string? Location { get; set; }
        public string? Mood { get; set; }
        public string? Duration { get; set; }
        public string? Weather { get; set; }
        public string? PriceCategory { get; set; } // "$", "$$", "$$$"
        public bool OnlyActive { get; set; } = true;
    }
}