using Microsoft.EntityFrameworkCore;
using ReviewService.Entities;

namespace ReviewService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Review>(entity =>
        {
            // ── Primary Key ───────────────────────────────────────────────
            entity.HasKey(r => r.ReviewId);

            entity.Property(r => r.ReviewId)
                .ValueGeneratedOnAdd();

            // ── Required Fields ───────────────────────────────────────────
            entity.Property(r => r.AppointmentId)
                .IsRequired();

            entity.Property(r => r.PatientId)
                .IsRequired();

            entity.Property(r => r.ProviderId)
                .IsRequired();

            entity.Property(r => r.Rating)
                .IsRequired();

            entity.Property(r => r.Comment)
                .HasMaxLength(1000);

            entity.Property(r => r.ReviewDate)
                .IsRequired();

            entity.Property(r => r.IsVerified)
                .HasDefaultValue(false);

            entity.Property(r => r.IsAnonymous)
                .HasDefaultValue(false);

            // ── Unique Constraint — one review per appointment ────────────
            entity.HasIndex(r => r.AppointmentId)
                .IsUnique();

            // ── Other Indexes ─────────────────────────────────────────────
            entity.HasIndex(r => r.ProviderId);
            entity.HasIndex(r => r.PatientId);
            entity.HasIndex(r => r.Rating);
        });
    }
} 
