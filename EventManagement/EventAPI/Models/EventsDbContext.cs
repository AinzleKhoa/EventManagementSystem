using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EventAPI.Models;

public partial class EventsDbContext : DbContext
{
    public EventsDbContext()
    {
    }

    public EventsDbContext(DbContextOptions<EventsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventStaff> EventStaffs { get; set; }

    public virtual DbSet<TicketType> TicketTypes { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__7944C810544FD418");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Latitude).HasColumnType("float");
            entity.Property(e => e.Longitude).HasColumnType("float");
            entity.Property(e => e.PosterUrl).HasMaxLength(500);
            entity.Property(e => e.RejectedReason).HasMaxLength(500);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Draft");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.ConversationId);
        });

        modelBuilder.Entity<EventStaff>(entity =>
        {
            entity.HasKey(e => e.EventStaffId).HasName("PK__EventSta__F07DF2418A9D00AF");

            entity.ToTable("EventStaff");

            entity.HasIndex(e => new { e.EventId, e.StaffEmail }, "UQ_Event_Staff_Email").IsUnique();

            entity.Property(e => e.StaffEmail).HasMaxLength(255);
            entity.Property(e => e.ValidatorRole).HasMaxLength(50);
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Event).WithMany(p => p.EventStaffs)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventStaf__Event__0F624AF8");
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.HasKey(e => e.TicketTypeId).HasName("PK__TicketTy__6CD684316E071C47");

            entity.Property(e => e.ColorCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoldQuantity).HasDefaultValue(0);
            entity.Property(e => e.TypeName).HasMaxLength(100);
            entity.Property(e => e.SaleStartDate).HasColumnType("datetime");
            entity.Property(e => e.SaleEndDate).HasColumnType("datetime");

            entity.HasOne(d => d.Event).WithMany(p => p.TicketTypes)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketTyp__Event__09A971A2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
