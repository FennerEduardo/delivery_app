namespace Shipping.Domain.ValueObjects;

public record Address(string Street, string City, string State, string ZipCode, string Country)
{
    public static Address Create(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Street cannot be empty.", nameof(street));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City cannot be empty.", nameof(city));
        return new Address(street.Trim(), city.Trim(), state?.Trim() ?? "", zipCode?.Trim() ?? "", country?.Trim() ?? "Colombia");
    }
}

public record Money(decimal Amount, string Currency = "COP")
{
    public static Money Zero(string currency = "COP") => new Money(0m, currency);
    public static Money From(decimal amount, string currency = "COP")
    {
        if (amount < 0) throw new ArgumentException("Money amount cannot be negative.", nameof(amount));
        return new Money(amount, currency);
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException("Currency mismatch.");
        return new Money(Amount + other.Amount, Currency);
    }
}

public record Weight(decimal Kilograms)
{
    public static Weight FromKg(decimal kg)
    {
        if (kg <= 0) throw new ArgumentException("Weight must be greater than zero.", nameof(kg));
        return new Weight(kg);
    }
}

public record Dimensions(decimal LengthCm, decimal WidthCm, decimal HeightCm)
{
    public const decimal DefaultVolumetricDivisor = 5000m;

    public static Dimensions Create(decimal length, decimal width, decimal height)
    {
        if (length <= 0 || width <= 0 || height <= 0)
            throw new ArgumentException("All package dimensions must be greater than zero.");
        return new Dimensions(length, width, height);
    }

    public decimal CalculateVolumetricWeight(decimal divisor = DefaultVolumetricDivisor)
    {
        if (divisor <= 0) throw new ArgumentException("Volumetric divisor must be greater than zero.", nameof(divisor));
        return (LengthCm * WidthCm * HeightCm) / divisor;
    }
}

public record Distance(decimal Kilometers)
{
    public static Distance FromKm(decimal km)
    {
        if (km < 0) throw new ArgumentException("Distance cannot be negative.", nameof(km));
        return new Distance(km);
    }
}
