using Microsoft.EntityFrameworkCore;
using ScheduleService.Entities;

namespace ScheduleService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AvailabilitySlot> AvailabilitySlots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AvailabilitySlot>(entity =>
        {
            // ── Primary Key ───────────────────────────────────────────────
            entity.HasKey(s => s.SlotId);

            // ── Primary Key Auto Increment ────────────────────────────────
            entity.Property(s => s.SlotId)
                .ValueGeneratedOnAdd();

            // ── Required Fields ───────────────────────────────────────────
            entity.Property(s => s.ProviderId)
                .IsRequired();

            entity.Property(s => s.Date)
                .IsRequired();

            entity.Property(s => s.StartTime)
                .IsRequired();

            entity.Property(s => s.EndTime)
                .IsRequired();

            entity.Property(s => s.DurationMinutes)
                .IsRequired();

            entity.Property(s => s.IsBooked)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(s => s.IsBlocked)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(s => s.Recurrence)
                .HasMaxLength(50)
                .HasDefaultValue("None");

            entity.Property(s => s.CreatedAt)
                .IsRequired();

            // ── Indexes ───────────────────────────────────────────────────
            entity.HasIndex(s => s.ProviderId);
            entity.HasIndex(s => s.Date);
        });
    }
}