namespace DateSpark.API.Models
{
    public class IdeaVote
    {
        public int IdeaId { get; set; }
        public bool IsLike { get; set; } // true = лайк, false = дизлайк
    }
}