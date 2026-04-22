using AppointmentService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Appointment>(entity =>
        {
            // ── Primary Key ───────────────────────────────────────────────
            entity.HasKey(a => a.AppointmentId);

            entity.Property(a => a.AppointmentId)
                .ValueGeneratedOnAdd();

            // ── Required Fields ───────────────────────────────────────────
            entity.Property(a => a.PatientId)
                .IsRequired();

            entity.Property(a => a.ProviderId)
                .IsRequired();

            entity.Property(a => a.SlotId)
                .IsRequired();

            entity.Property(a => a.ServiceType)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(a => a.AppointmentDate)
                .IsRequired();

            entity.Property(a => a.StartTime)
                .IsRequired();

            entity.Property(a => a.EndTime)
                .IsRequired();

            entity.Property(a => a.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Scheduled");

            entity.Property(a => a.ModeOfConsultation)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("InPerson");

            entity.Property(a => a.CreatedAt)
                .IsRequired();

            entity.Property(a => a.UpdatedAt)
                .IsRequired();

            // ── Indexes ───────────────────────────────────────────────────
            entity.HasIndex(a => a.PatientId);
            entity.HasIndex(a => a.ProviderId);
            entity.HasIndex(a => a.SlotId);
            entity.HasIndex(a => a.Status);
        });
    }
}
