using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;

namespace NotificationService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            // ── Primary Key ───────────────────────────────────────────────
            entity.HasKey(n => n.NotificationId);

            entity.Property(n => n.NotificationId)
                .ValueGeneratedOnAdd();

            // ── Required Fields ───────────────────────────────────────────
            entity.Property(n => n.RecipientId)
                .IsRequired();

            entity.Property(n => n.Type)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(n => n.Channel)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("APP");

            entity.Property(n => n.RelatedType)
                .HasMaxLength(100);

            entity.Property(n => n.IsRead)
                .HasDefaultValue(false);

            entity.Property(n => n.SentAt)
                .IsRequired();

            // ── Indexes ───────────────────────────────────────────────────
            entity.HasIndex(n => n.RecipientId);
            entity.HasIndex(n => n.Type);
            entity.HasIndex(n => n.IsRead);
            entity.HasIndex(n => n.RelatedId);
        });
    }
}
