using Shipping.Domain.Enums;
using Shipping.Domain.Models;

namespace Shipping.Domain.Services.Pricing.Rules;

public class WeightTierPricingRule : IPricingRule
{
    public string RuleId => "RULE_WEIGHT_TIER";
    public string RuleName => "Base Rate per Billable Weight Tier";
    public int ExecutionOrder => 1;

    public QuoteComponentBreakdown Evaluate(PricingRuleEvaluationContext context)
    {
        decimal billableKg = context.BillableWeightKg;
        decimal baseCostAmount;
        string baseRule;

        if (billableKg <= 2.0m)
        {
            baseCostAmount = 10000m;
            baseRule = "Tier 0-2 kg -> 10,000 COP";
        }
        else if (billableKg <= 5.0m)
        {
            baseCostAmount = 15000m;
            baseRule = "Tier >2-5 kg -> 15,000 COP";
        }
        else if (billableKg <= 10.0m)
        {
            baseCostAmount = 22000m;
            baseRule = "Tier >5-10 kg -> 22,000 COP";
        }
        else if (billableKg <= 20.0m)
        {
            baseCostAmount = 35000m;
            baseRule = "Tier >10-20 kg -> 35,000 COP";
        }
        else
        {
            decimal extraKg = Math.Ceiling(billableKg - 20.0m);
            baseCostAmount = 35000m + (extraKg * 2000m);
            baseRule = $"Tier >20 kg -> 35,000 + ({extraKg} kg x 2,000 COP)";
        }

        return new QuoteComponentBreakdown("BaseCost", "Base rate per billable weight", baseCostAmount, 0m, baseRule);
    }
}

public class DistanceSurchargeRule : IPricingRule
{
    public string RuleId => "RULE_DISTANCE_SURCHARGE";
    public string RuleName => "Distance Range Percentage Surcharge";
    public int ExecutionOrder => 2;

    public QuoteComponentBreakdown Evaluate(PricingRuleEvaluationContext context)
    {
        decimal distanceKm = context.Distance.Kilometers;
        decimal distancePct;
        string distanceRule;

        if (distanceKm <= 10m)
        {
            distancePct = 0m;
            distanceRule = "Distance 0-10 km -> 0%";
        }
        else if (distanceKm <= 30m)
        {
            distancePct = 0.10m;
            distanceRule = "Distance >10-30 km -> +10%";
        }
        else if (distanceKm <= 80m)
        {
            distancePct = 0.20m;
            distanceRule = "Distance >30-80 km -> +20%";
        }
        else if (distanceKm <= 150m)
        {
            distancePct = 0.35m;
            distanceRule = "Distance >80-150 km -> +35%";
        }
        else
        {
            distancePct = 0.50m;
            distanceRule = "Distance >150 km -> +50%";
        }

        decimal amount = context.CurrentSubtotal * distancePct;
        return new QuoteComponentBreakdown("DistanceSurcharge", "Distance surcharge", amount, distancePct * 100m, distanceRule);
    }
}

public class CommercialValueSurchargeRule : IPricingRule
{
    public string RuleId => "RULE_COMMERCIAL_VALUE_SURCHARGE";
    public string RuleName => "Declared Commercial Value Surcharge";
    public int ExecutionOrder => 3;

    public QuoteComponentBreakdown Evaluate(PricingRuleEvaluationContext context)
    {
        decimal valAmount = context.CommercialValue.Amount;
        decimal valPct;
        string valRule;

        if (valAmount <= 500000m)
        {
            valPct = 0m;
            valRule = "Commercial value <= 500k -> 0%";
        }
        else if (valAmount <= 2000000m)
        {
            valPct = 0.01m;
            valRule = "Commercial value 500k-2M -> +1%";
        }
        else if (valAmount <= 5000000m)
        {
            valPct = 0.02m;
            valRule = "Commercial value 2M-5M -> +2%";
        }
        else
        {
            valPct = 0.03m;
            valRule = "Commercial value > 5M -> +3%";
        }

        decimal amount = context.CurrentSubtotal * valPct;
        return new QuoteComponentBreakdown("CommercialValueSurcharge", "Declared commercial value surcharge", amount, valPct * 100m, valRule);
    }
}

public class DeliveryTypeSurchargeRule : IPricingRule
{
    public string RuleId => "RULE_DELIVERY_TYPE_SURCHARGE";
    public string RuleName => "Delivery Type Speed Multiplier";
    public int ExecutionOrder => 4;

    public QuoteComponentBreakdown Evaluate(PricingRuleEvaluationContext context)
    {
        decimal typePct;
        string typeRule;

        switch (context.DeliveryType)
        {
            case DeliveryType.Express:
                typePct = 0.30m;
                typeRule = "Express delivery -> +30%";
                break;
            case DeliveryType.SameDay:
                typePct = 0.60m;
                typeRule = "Same-day delivery -> +60%";
                break;
            case DeliveryType.Standard:
            default:
                typePct = 0m;
                typeRule = "Standard delivery -> 0%";
                break;
        }

        decimal amount = context.CurrentSubtotal * typePct;
        return new QuoteComponentBreakdown("DeliveryTypeSurcharge", "Delivery type speed multiplier", amount, typePct * 100m, typeRule);
    }
}

public class DeliveryWindowSurchargeRule : IPricingRule
{
    public string RuleId => "RULE_DELIVERY_WINDOW_SURCHARGE";
    public string RuleName => "Time Window Delivery Surcharge";
    public int ExecutionOrder => 5;

    public QuoteComponentBreakdown Evaluate(PricingRuleEvaluationContext context)
    {
        decimal windowPct;
        string windowRule;

        switch (context.TimeWindow)
        {
            case DeliveryWindowType.Extended:
                windowPct = 0.10m;
                windowRule = "Extended window -> +10%";
                break;
            case DeliveryWindowType.Night:
                windowPct = 0.20m;
                windowRule = "Night delivery -> +20%";
                break;
            case DeliveryWindowType.Weekend:
                windowPct = 0.25m;
                windowRule = "Weekend delivery -> +25%";
                break;
            case DeliveryWindowType.Standard:
            default:
                windowPct = 0m;
                windowRule = "Standard window -> 0%";
                break;
        }

        decimal amount = context.CurrentSubtotal * windowPct;
        return new QuoteComponentBreakdown("TimeWindowSurcharge", "Time window delivery surcharge", amount, windowPct * 100m, windowRule);
    }
}
