namespace DateSpark.API.Models
{
    public class VoteRequest
    {
        public int IdeaId { get; set; }
        public bool IsLike { get; set; }
    }
}