using Microsoft.EntityFrameworkCore;

namespace WithdrawalAPI.Models
{
    public partial class WithdrawalDbContext : DbContext
    {
        public WithdrawalDbContext(DbContextOptions<WithdrawalDbContext> options) : base(options) { }

        public virtual DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WithdrawalRequest>(entity =>
            {
                entity.HasKey(e => e.WithdrawalId);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.BankName)
                    .HasMaxLength(100);

                entity.Property(e => e.BankAccountNumber)
                    .HasMaxLength(50);

                entity.Property(e => e.BankAccountName)
                    .HasMaxLength(100);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("Pending");

                entity.Property(e => e.RequestedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.RejectReason)
                    .HasMaxLength(500);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
