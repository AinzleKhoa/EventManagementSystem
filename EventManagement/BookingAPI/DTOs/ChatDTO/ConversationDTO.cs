namespace BookingAPI.DTOs.ChatDTO
{
    /// <summary>
    /// DTO for conversation information
    /// </summary>
    public class ConversationDTO
    {
        public int ConversationId { get; set; }
        public string Type { get; set; } // "Private" or "Group"
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public int? EventId { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ParticipantCount { get; set; }
        public int MessageCount { get; set; }
    }

    /// <summary>
    /// DTO to add a participant to a conversation
    /// </summary>
    public class AddConversationParticipantDTO
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// DTO to create a new conversation
    /// </summary>
    public class CreateConversationDTO
    {
        public string Type { get; set; } // "Private" or "Group"
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public int? EventId { get; set; }
        public List<int> ParticipantIds { get; set; } = new();
    }
}
