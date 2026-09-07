namespace ChatAPI.DTOs
{
    public class AddConversationParticipantDTO
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
    }

    public class ConversationParticipantDTO
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsMuted { get; set; }
        public int? LastReadMessageId { get; set; }
    }

    public class AssignRoleDTO
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } 
    }

    public class UpdateParticipantDTO
    {
        public bool? IsMuted { get; set; }
        public int? LastReadMessageId { get; set; }
    }
}
