using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shipping.Domain.Entities;
using Shipping.Domain.ValueObjects;
using Shipping.Infrastructure.Persistence;

namespace Shipping.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Build an intermediate service provider to seed data after EnsureCreated
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
