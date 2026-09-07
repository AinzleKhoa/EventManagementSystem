namespace ChatAPI.DTOs
{
    public class CreateConversationDTO
    {
        public string Type { get; set; } 
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public int? EventId { get; set; }
        public List<int> ParticipantIds { get; set; } = new();
    }

    public class ConversationDTO
    {
        public int ConversationId { get; set; }
        public string Type { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public int? EventId { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ParticipantCount { get; set; }
        public int MessageCount { get; set; }
    }

    public class UpdateConversationDTO
    {
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
    }
}
