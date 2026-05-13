using Microsoft.EntityFrameworkCore;
using PaymentService.Entities;

namespace PaymentService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Payment>(entity =>
        {
            // ── Primary Key ───────────────────────────────────────────────
            entity.HasKey(p => p.PaymentId);

            entity.Property(p => p.PaymentId)
                .ValueGeneratedOnAdd();

            // ── Required Fields ───────────────────────────────────────────
            entity.Property(p => p.AppointmentId)
                .IsRequired();

            entity.Property(p => p.PatientId)
                .IsRequired();

            entity.Property(p => p.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.Property(p => p.Mode)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Card");

            entity.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("USD");

            entity.Property(p => p.TransactionId)
                .HasMaxLength(200);

            // ── Indexes ───────────────────────────────────────────────────
            entity.HasIndex(p => p.AppointmentId);
            entity.HasIndex(p => p.PatientId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.TransactionId);
        });
    }
} 
