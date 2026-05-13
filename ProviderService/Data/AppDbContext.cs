using Microsoft.EntityFrameworkCore;
using ProviderService.Entities;

namespace ProviderService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Provider> Providers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Provider>()
            .Property(p => p.Specialization)
            .HasMaxLength(150);

        modelBuilder.Entity<Provider>()
            .Property(p => p.Qualification)
            .HasMaxLength(200);

        modelBuilder.Entity<Provider>()
            .Property(p => p.ClinicName)
            .HasMaxLength(200);

        modelBuilder.Entity<Provider>()
            .Property(p => p.ClinicAddress)
            .HasMaxLength(300);
    }
}