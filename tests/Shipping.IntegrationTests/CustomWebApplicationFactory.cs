using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shipping.Domain.Entities;
using Shipping.Domain.ValueObjects;
using Shipping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Shipping.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Use a unique DB name for isolated test runs
            var dbName = $"ShippingDb_{Guid.NewGuid()}";
            
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ShippingDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            
            services.AddDbContext<ShippingDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();

            db.Database.EnsureCreated();

            // Seed test data if empty
            if (!db.Customers.Any())
            {
                var customer = Customer.Create(
                    "Test Customer",
                    "test@example.com",
                    "+57 300 123 4567",
                    new Address("Calle 100 #15-20", "Bogotá", "Cundinamarca", "110111", "Colombia"));

                db.Customers.Add(customer);
                db.SaveChanges();
            }
        });
    }
}
