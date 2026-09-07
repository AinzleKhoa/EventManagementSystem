namespace ChatAPI.DTOs
{
    public class CreateMessageDTO
    {
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string? SenderName { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } = "Text";
        public string? AttachmentUrl { get; set; }
    }

    public class MessageDTO
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string? SenderName { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class UpdateMessageDTO
    {
        public string Content { get; set; }
    }
}
