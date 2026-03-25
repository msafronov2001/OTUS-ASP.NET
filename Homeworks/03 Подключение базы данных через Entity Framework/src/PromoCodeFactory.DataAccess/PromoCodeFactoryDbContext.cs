using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System.Data;

namespace PromoCodeFactory.DataAccess;

public class PromoCodeFactoryDbContext : DbContext
{
    public PromoCodeFactoryDbContext(DbContextOptions<PromoCodeFactoryDbContext> options)
        : base(options)
    {
    }

    //TODO: Добавить DbSet сущности

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerPromoCode> CustomerPromoCodes { get; set; }
    public DbSet<Preference> Preferences { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //TODO: Добавить маппинг моделей

        modelBuilder.Entity<Employee>(emp =>
        {
            emp.HasKey(e => e.Id);

            emp.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            emp.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);
            emp.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            emp.HasOne(e => e.Role)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(role =>
        {
            role.HasKey(r => r.Id);

            role.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);
            role.Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Customer>(cust =>
        {
            cust.HasKey(c => c.Id);

            cust.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            cust.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(50);
            cust.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(256);

            cust.HasMany(p => p.Preferences)
                .WithMany(c => c.Customers)
                .UsingEntity(c => c.ToTable("CustomerPreferences"));

            cust.HasMany(cpc => cpc.CustomerPromoCodes)
                .WithOne()
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerPromoCode>(cpc =>
        {
            cpc.HasKey(c => c.Id);

            cpc.Property(c => c.CreatedAt)
                .IsRequired();

            cpc.Property(c => c.AppliedAt);

            cpc.HasIndex(c => c.CustomerId);
            cpc.HasIndex(c => c.PromoCodeId);

            cpc.HasIndex(c => new { c.CustomerId, c.PromoCodeId })
                .IsUnique();
        });

        modelBuilder.Entity<Preference>(pref =>
        {
            pref.HasKey(p => p.Id);

            pref.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

        });

        modelBuilder.Entity<PromoCode>(pc =>
        {
            pc.HasKey(p => p.Id);

            pc.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(100);
            pc.Property(p => p.ServiceInfo)
                .IsRequired()
                .HasMaxLength(100);
            pc.Property(p => p.PartnerName)
                .IsRequired()
                .HasMaxLength(100);
            pc.Property(p => p.BeginDate).IsRequired();
            pc.Property(p => p.EndDate).IsRequired();

            pc.HasOne(p => p.PartnerManager)
                .WithMany()
                .IsRequired();

            pc.HasOne(p => p.Preference)
                .WithMany()
                .IsRequired();

            pc.HasMany(p => p.CustomerPromoCodes)
                .WithOne()
                .HasForeignKey(p => p.PromoCodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}
