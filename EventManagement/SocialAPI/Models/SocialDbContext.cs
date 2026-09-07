using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SocialAPI.Models;

public partial class SocialDbContext : DbContext
{
    public SocialDbContext()
    {
    }

    public SocialDbContext(DbContextOptions<SocialDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CE44E60DA8");

            entity.HasIndex(e => new { e.UserId, e.EventId }, "UQ_Reviews_User_Event").IsUnique();

            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.WishlistId).HasName("PK__Wishlist__233189EB5E7F43B5");

            entity.HasIndex(e => new { e.UserId, e.EventId }, "UQ__Wishlist__001C80CCA88385B7").IsUnique();

            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
