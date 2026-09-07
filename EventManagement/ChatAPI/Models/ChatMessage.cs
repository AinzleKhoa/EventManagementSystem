using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAPI.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }

        [Required]
        public int SenderId { get; set; } 

        [MaxLength(255)]
        public string? SenderName { get; set; } 

        [Required]
        public string Content { get; set; }

        [MaxLength(20)]
        public string MessageType { get; set; } = "Text"; 

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;
    }
}
