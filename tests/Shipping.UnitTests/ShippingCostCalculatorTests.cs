using FluentAssertions;
using Shipping.Domain.Enums;
using Shipping.Domain.Services.Pricing;
using Shipping.Domain.ValueObjects;
using Xunit;

namespace Shipping.UnitTests;

public class ShippingCostCalculatorTests
{
    private readonly ShippingCostCalculator _calculator = new();

    [Fact]
    public void CalculateQuote_StandardLightweightShipment_ReturnsExpectedBaseAndTotalCost()
    {
        // Arrange (Given a shipment weighing 3 kg, 20x15x10 cm, value 200,000 COP, distance 25 km, Standard type & window)
        var weight = Weight.FromKg(3.0m);
        var dims = Dimensions.Create(20, 15, 10); // Volumetric weight = (20*15*10)/5000 = 0.6 kg -> Billable weight = 3.0 kg
        var value = Money.From(200000m);
        var distance = Distance.FromKm(25m);

        // Act (When quote is calculated)
        var quote = _calculator.CalculateQuote(weight, dims, value, distance, DeliveryType.Standard, DeliveryWindowType.Standard);

        // Assert (Then billable weight = 3.0 kg, base cost = 15,000, distance surcharge = 1,500 (+10%), total = 16,500)
        quote.BillableWeightKg.Should().Be(3.0m);
        quote.BaseCost.Amount.Should().Be(15000m);
        quote.DistanceSurcharge.Amount.Should().Be(1500m);
        quote.CommercialValueSurcharge.Amount.Should().Be(0m);
        quote.Total.Amount.Should().Be(16500m);
        quote.BreakdownComponents.Should().NotBeEmpty();
    }

    [Fact]
    public void CalculateQuote_VolumetricWeightExceedsActualWeight_UsesVolumetricAsBillableWeight()
    {
        // Arrange (Actual weight 2 kg, dimensions 50x40x30 cm -> Volumetric = 16 kg)
        var weight = Weight.FromKg(2.0m);
        var dims = Dimensions.Create(50, 40, 30);
        var value = Money.From(100000m);
        var distance = Distance.FromKm(5m);

        // Act
        var quote = _calculator.CalculateQuote(weight, dims, value, distance, DeliveryType.Standard, DeliveryWindowType.Standard);

        // Assert (Billable weight should be 16 kg -> Tier >10-20 kg = 35,000 COP base cost)
        quote.VolumetricWeightKg.Should().Be(16.0m);
        quote.BillableWeightKg.Should().Be(16.0m);
        quote.BaseCost.Amount.Should().Be(35000m);
        quote.Total.Amount.Should().Be(35000m);
    }

    [Theory]
    [InlineData(1.0, 10000)]  // 0-2 kg -> 10k
    [InlineData(4.0, 15000)]  // >2-5 kg -> 15k
    [InlineData(8.0, 22000)]  // >5-10 kg -> 22k
    [InlineData(15.0, 35000)] // >10-20 kg -> 35k
    [InlineData(22.0, 39000)] // >20 kg -> 35k + (2 * 2k) = 39k
    public void CalculateQuote_WeightTiers_ReturnsCorrectBaseCost(decimal kg, decimal expectedBaseCost)
    {
        var weight = Weight.FromKg(kg);
        var dims = Dimensions.Create(10, 10, 10);
        var value = Money.From(0m);
        var distance = Distance.FromKm(5m);

        var quote = _calculator.CalculateQuote(weight, dims, value, distance, DeliveryType.Standard, DeliveryWindowType.Standard);

        quote.BaseCost.Amount.Should().Be(expectedBaseCost);
    }

    [Theory]
    [InlineData(5, 0.0)]     // 0-10 km -> 0%
    [InlineData(20, 0.10)]   // >10-30 km -> +10%
    [InlineData(50, 0.20)]   // >30-80 km -> +20%
    [InlineData(100, 0.35)]  // >80-150 km -> +35%
    [InlineData(200, 0.50)]  // >150 km -> +50%
    public void CalculateQuote_DistanceRanges_AppliesCorrectPercentage(decimal km, decimal expectedPct)
    {
        var weight = Weight.FromKg(1.0m); // Base cost 10,000
        var dims = Dimensions.Create(10, 10, 10);
        var value = Money.From(0m);
        var distance = Distance.FromKm(km);

        var quote = _calculator.CalculateQuote(weight, dims, value, distance, DeliveryType.Standard, DeliveryWindowType.Standard);

        decimal expectedSurcharge = 10000m * expectedPct;
        quote.DistanceSurcharge.Amount.Should().Be(expectedSurcharge);
    }
}
