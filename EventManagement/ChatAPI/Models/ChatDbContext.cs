using Microsoft.EntityFrameworkCore;
using ChatAPI.Models;

namespace ChatAPI.Models
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversation configurations
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.ConversationId);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.LastMessageAt).HasDefaultValueSql("GETDATE()");

                entity.HasMany(e => e.Messages)
                    .WithOne(m => m.Conversation)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Participants)
                    .WithOne(p => p.Conversation)
                    .HasForeignKey(p => p.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.MessageId);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.SenderName).HasMaxLength(255);
                entity.Property(e => e.MessageType).HasMaxLength(20).HasDefaultValue("Text");
                entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
                entity.Property(e => e.SentAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasIndex(e => e.ConversationId);
                entity.HasIndex(e => new { e.ConversationId, e.SentAt });
            });

            modelBuilder.Entity<ConversationParticipant>(entity =>
            {
                entity.HasKey(e => new { e.ConversationId, e.UserId });
                entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue("Member");
                entity.Property(e => e.JoinedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsMuted).HasDefaultValue(false);

                entity.HasIndex(e => e.UserId);
            });
        }
    }
}
