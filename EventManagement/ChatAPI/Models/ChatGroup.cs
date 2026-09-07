using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAPI.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } 

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int? EventId { get; set; } 

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    }
}

