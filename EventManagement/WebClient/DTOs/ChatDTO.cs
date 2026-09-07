namespace WebClient.DTOs
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

    /// <summary>
    /// DTO to update a conversation
    /// </summary>
    public class UpdateConversationDTO
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
    }

    /// <summary>
    /// DTO for conversation participant
    /// </summary>
    public class ConversationParticipantDTO
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsMuted { get; set; }
        public int? LastReadMessageId { get; set; }
    }

    /// <summary>
    /// DTO to add a participant
    /// </summary>
    public class AddConversationParticipantDTO
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// DTO to assign role
    /// </summary>
    public class AssignRoleDTO
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; }
    }

    /// <summary>
    /// DTO for message
    /// </summary>
    public class MessageDTO
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; }
        public string AttachmentUrl { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// DTO to create a message
    /// </summary>
    public class CreateMessageDTO
    {
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } = "Text";
        public string AttachmentUrl { get; set; }
    }

    /// <summary>
    /// DTO to update a message
    /// </summary>
    public class UpdateMessageDTO
    {
        public string Content { get; set; }
    }
}
