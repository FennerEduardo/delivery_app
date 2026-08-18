using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private Customer() { } // EF Core

    public static Customer Create(string name, string email, string phone, Address address)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Customer name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Customer email is required.", nameof(email));

        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone?.Trim() ?? string.Empty,
            Address = address,
            CreatedAt = DateTime.UtcNow
        };
    }
}
