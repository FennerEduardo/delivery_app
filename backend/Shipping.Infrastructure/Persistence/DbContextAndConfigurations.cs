using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shipping.Domain.Entities;
using Shipping.Domain.Models;
using Shipping.Domain.ValueObjects;

namespace Shipping.Infrastructure.Persistence;

public class ShippingDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentStatusHistory> StatusHistories => Set<ShipmentStatusHistory>();

    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer Configuration
        modelBuilder.Entity<Customer>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).IsRequired().HasMaxLength(200);
            b.Property(c => c.Email).IsRequired().HasMaxLength(200);
            b.Property(c => c.Phone).HasMaxLength(50);

            b.OwnsOne(c => c.Address, a =>
            {
                a.Property(ad => ad.Street).HasColumnName("Street").HasMaxLength(250);
                a.Property(ad => ad.City).HasColumnName("City").HasMaxLength(100);
                a.Property(ad => ad.State).HasColumnName("State").HasMaxLength(100);
                a.Property(ad => ad.ZipCode).HasColumnName("ZipCode").HasMaxLength(20);
                a.Property(ad => ad.Country).HasColumnName("Country").HasMaxLength(100);
            });
        });

        // Shipment Configuration
        modelBuilder.Entity<Shipment>(b =>
        {
            b.HasKey(s => s.Id);
            if (this.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                b.Property(s => s.RowVersion).IsRowVersion(); // Optimistic Concurrency
            }

            b.OwnsOne(s => s.Origin, a =>
            {
                a.Property(ad => ad.Street).HasColumnName("OriginStreet").HasMaxLength(250);
                a.Property(ad => ad.City).HasColumnName("OriginCity").HasMaxLength(100);
                a.Property(ad => ad.State).HasColumnName("OriginState").HasMaxLength(100);
                a.Property(ad => ad.ZipCode).HasColumnName("OriginZipCode").HasMaxLength(20);
                a.Property(ad => ad.Country).HasColumnName("OriginCountry").HasMaxLength(100);
            });

            b.OwnsOne(s => s.Destination, a =>
            {
                a.Property(ad => ad.Street).HasColumnName("DestStreet").HasMaxLength(250);
                a.Property(ad => ad.City).HasColumnName("DestCity").HasMaxLength(100);
                a.Property(ad => ad.State).HasColumnName("DestState").HasMaxLength(100);
                a.Property(ad => ad.ZipCode).HasColumnName("DestZipCode").HasMaxLength(20);
                a.Property(ad => ad.Country).HasColumnName("DestCountry").HasMaxLength(100);
            });

            b.OwnsOne(s => s.Weight, w =>
            {
                w.Property(weight => weight.Kilograms).HasColumnName("WeightKg").HasPrecision(18, 2);
            });

            b.OwnsOne(s => s.Dimensions, d =>
            {
                d.Property(dim => dim.LengthCm).HasColumnName("LengthCm").HasPrecision(18, 2);
                d.Property(dim => dim.WidthCm).HasColumnName("WidthCm").HasPrecision(18, 2);
                d.Property(dim => dim.HeightCm).HasColumnName("HeightCm").HasPrecision(18, 2);
            });

            b.OwnsOne(s => s.CommercialValue, m =>
            {
                m.Property(mon => mon.Amount).HasColumnName("CommercialValueAmount").HasPrecision(18, 2);
                m.Property(mon => mon.Currency).HasColumnName("CommercialValueCurrency").HasMaxLength(10);
            });

            b.OwnsOne(s => s.Distance, dist =>
            {
                dist.Property(d => d.Kilometers).HasColumnName("DistanceKm").HasPrecision(18, 2);
            });

            b.OwnsOne(s => s.BaseCost, m =>
            {
                m.Property(mon => mon.Amount).HasColumnName("BaseCostAmount").HasPrecision(18, 2);
                m.Property(mon => mon.Currency).HasColumnName("BaseCostCurrency").HasMaxLength(10);
            });

            b.OwnsOne(s => s.TotalCost, m =>
            {
                m.Property(mon => mon.Amount).HasColumnName("TotalCostAmount").HasPrecision(18, 2);
                m.Property(mon => mon.Currency).HasColumnName("TotalCostCurrency").HasMaxLength(10);
            });

            // Serialize QuoteDetails to JSON column
            b.Property(s => s.QuoteDetails)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<ShippingQuote>(v, (JsonSerializerOptions?)null)
                );

            var historyNavigation = b.HasMany(s => s.StatusHistory)
                .WithOne()
                .HasForeignKey(h => h.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            historyNavigation.Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ShipmentStatusHistory>(b =>
        {
            b.HasKey(h => h.Id);
            b.Property(h => h.Comment).HasMaxLength(500);
        });
    }
}
