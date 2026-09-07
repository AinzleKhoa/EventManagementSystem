using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAPI.Models
{
    public class ConversationParticipant
    {
        [Required]
        public int ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }

        [Required]
        public int UserId { get; set; } 

        [MaxLength(20)]
        public string Role { get; set; } = "Member";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsMuted { get; set; } = false;

        public int? LastReadMessageId { get; set; }

        [Key]
        public string Id => $"{ConversationId}_{UserId}";
    }
}
