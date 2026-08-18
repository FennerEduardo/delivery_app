using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Models;

public record QuoteComponentBreakdown(
    string ComponentName,
    string Description,
    decimal Amount,
    decimal Percentage,
    string RuleApplied
);

public class ShippingQuote
{
    public Money BaseCost { get; set; } = Money.Zero();
    public Money WeightSurcharge { get; set; } = Money.Zero();
    public Money DistanceSurcharge { get; set; } = Money.Zero();
    public Money CommercialValueSurcharge { get; set; } = Money.Zero();
    public Money DeliveryTypeSurcharge { get; set; } = Money.Zero();
    public Money TimeWindowSurcharge { get; set; } = Money.Zero();
    public Money Discount { get; set; } = Money.Zero();
    public Money Total { get; set; } = Money.Zero();

    public decimal ActualWeightKg { get; set; }
    public decimal VolumetricWeightKg { get; set; }
    public decimal BillableWeightKg { get; set; }

    public List<QuoteComponentBreakdown> BreakdownComponents { get; set; } = new();
}
