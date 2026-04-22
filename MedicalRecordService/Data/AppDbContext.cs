using MedicalRecordService.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<MedicalRecord> MedicalRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            // ── Primary Key ───────────────────────────────────────────────
            entity.HasKey(r => r.RecordId);

            entity.Property(r => r.RecordId)
                .ValueGeneratedOnAdd();

            // ── Required Fields ───────────────────────────────────────────
            entity.Property(r => r.AppointmentId)
                .IsRequired();

            entity.Property(r => r.PatientId)
                .IsRequired();

            entity.Property(r => r.ProviderId)
                .IsRequired();

            entity.Property(r => r.Diagnosis)
                .IsRequired();

            entity.Property(r => r.AttachmentUrl)
                .HasMaxLength(500);

            entity.Property(r => r.CreatedAt)
                .IsRequired();

            entity.Property(r => r.UpdatedAt)
                .IsRequired();

            // ── Unique Constraint — one record per appointment ────────────
            entity.HasIndex(r => r.AppointmentId)
                .IsUnique();

            // ── Other Indexes ─────────────────────────────────────────────
            entity.HasIndex(r => r.PatientId);
            entity.HasIndex(r => r.ProviderId);
            entity.HasIndex(r => r.FollowUpDate);
        });
    }
} 
