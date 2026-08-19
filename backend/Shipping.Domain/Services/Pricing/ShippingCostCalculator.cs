using Shipping.Domain.Enums;
using Shipping.Domain.Models;
using Shipping.Domain.Services.Pricing.Rules;
using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Services.Pricing;

public class ShippingCostCalculator : IShippingCostCalculator
{
    private readonly decimal _volumetricDivisor;
    private readonly List<IPricingRule> _rules;
    public string CurrentPricingVersion { get; } = "2026.08";

    public ShippingCostCalculator(
        IEnumerable<IPricingRule>? customRules = null,
        decimal volumetricDivisor = Dimensions.DefaultVolumetricDivisor)
    {
        _volumetricDivisor = volumetricDivisor > 0 ? volumetricDivisor : Dimensions.DefaultVolumetricDivisor;

        _rules = customRules != null && customRules.Any()
            ? customRules.OrderBy(r => r.ExecutionOrder).ToList()
            : GetDefaultRules();
    }

    private static List<IPricingRule> GetDefaultRules() => new List<IPricingRule>
    {
        new WeightTierPricingRule(),
        new DistanceSurchargeRule(),
        new CommercialValueSurchargeRule(),
        new DeliveryTypeSurchargeRule(),
        new DeliveryWindowSurchargeRule()
    }.OrderBy(r => r.ExecutionOrder).ToList();

    public ShippingQuote CalculateQuote(
        Weight actualWeight,
        Dimensions dimensions,
        Money commercialValue,
        Distance distance,
        DeliveryType deliveryType,
        DeliveryWindowType timeWindow)
    {
        var quote = new ShippingQuote
        {
            PricingVersion = CurrentPricingVersion,
            QuotedAt = DateTime.UtcNow
        };

        var breakdowns = new List<QuoteComponentBreakdown>();
        var appliedRuleIds = new List<string>();

        // 1. Calculate Weights
        decimal actualKg = actualWeight.Kilograms;
        decimal volumetricKg = dimensions.CalculateVolumetricWeight(_volumetricDivisor);
        decimal billableKg = Math.Max(actualKg, volumetricKg);

        quote.ActualWeightKg = actualKg;
        quote.VolumetricWeightKg = Math.Round(volumetricKg, 2);
        quote.BillableWeightKg = Math.Round(billableKg, 2);

        decimal currentSubtotal = 0m;
        decimal baseCostAmount = 0m;
        decimal distanceSurchargeAmount = 0m;
        decimal valSurchargeAmount = 0m;
        decimal typeSurchargeAmount = 0m;
        decimal windowSurchargeAmount = 0m;

        // 2. Pipeline Rule Evaluation
        foreach (var rule in _rules)
        {
            var ctx = new PricingRuleEvaluationContext(
                actualWeight,
                dimensions,
                commercialValue,
                distance,
                deliveryType,
                timeWindow,
                actualKg,
                volumetricKg,
                billableKg,
                currentSubtotal
            );

            var result = rule.Evaluate(ctx);
            appliedRuleIds.Add(rule.RuleId);

            switch (result.ComponentName)
            {
                case "BaseCost":
                    baseCostAmount = result.Amount;
                    currentSubtotal = baseCostAmount;
                    quote.BaseCost = Money.From(baseCostAmount);
                    breakdowns.Add(result);
                    break;
                case "DistanceSurcharge":
                    distanceSurchargeAmount = result.Amount;
                    quote.DistanceSurcharge = Money.From(distanceSurchargeAmount);
                    if (distanceSurchargeAmount > 0) breakdowns.Add(result);
                    break;
                case "CommercialValueSurcharge":
                    valSurchargeAmount = result.Amount;
                    quote.CommercialValueSurcharge = Money.From(valSurchargeAmount);
                    if (valSurchargeAmount > 0) breakdowns.Add(result);
                    break;
                case "DeliveryTypeSurcharge":
                    // Subtotal before delivery speed multiplier
                    currentSubtotal = baseCostAmount + distanceSurchargeAmount + valSurchargeAmount;
                    var typeCtx = ctx with { CurrentSubtotal = currentSubtotal };
                    result = rule.Evaluate(typeCtx);
                    typeSurchargeAmount = result.Amount;
                    quote.DeliveryTypeSurcharge = Money.From(typeSurchargeAmount);
                    if (typeSurchargeAmount > 0) breakdowns.Add(result);
                    break;
                case "TimeWindowSurcharge":
                    currentSubtotal = baseCostAmount + distanceSurchargeAmount + valSurchargeAmount;
                    var windowCtx = ctx with { CurrentSubtotal = currentSubtotal };
                    result = rule.Evaluate(windowCtx);
                    windowSurchargeAmount = result.Amount;
                    quote.TimeWindowSurcharge = Money.From(windowSurchargeAmount);
                    if (windowSurchargeAmount > 0) breakdowns.Add(result);
                    break;
            }
        }

        // 3. Final Total Calculation
        decimal totalAmount = baseCostAmount + distanceSurchargeAmount + valSurchargeAmount + typeSurchargeAmount + windowSurchargeAmount;
        quote.Total = Money.From(totalAmount);
        quote.AppliedRuleIds = appliedRuleIds;
        quote.BreakdownComponents = breakdowns;

        return quote;
    }
}
