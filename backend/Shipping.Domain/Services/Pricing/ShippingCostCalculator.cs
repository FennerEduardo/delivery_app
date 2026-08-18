using Shipping.Domain.Enums;
using Shipping.Domain.Models;
using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Services.Pricing;

public class ShippingCostCalculator : IShippingCostCalculator
{
    private readonly decimal _volumetricDivisor;

    public ShippingCostCalculator(decimal volumetricDivisor = Dimensions.DefaultVolumetricDivisor)
    {
        _volumetricDivisor = volumetricDivisor > 0 ? volumetricDivisor : Dimensions.DefaultVolumetricDivisor;
    }

    public ShippingQuote CalculateQuote(
        Weight actualWeight,
        Dimensions dimensions,
        Money commercialValue,
        Distance distance,
        DeliveryType deliveryType,
        DeliveryWindowType timeWindow)
    {
        var quote = new ShippingQuote();
        var breakdowns = new List<QuoteComponentBreakdown>();

        // 1. Calculate Weights
        decimal actualKg = actualWeight.Kilograms;
        decimal volumetricKg = dimensions.CalculateVolumetricWeight(_volumetricDivisor);
        decimal billableKg = Math.Max(actualKg, volumetricKg);

        quote.ActualWeightKg = actualKg;
        quote.VolumetricWeightKg = Math.Round(volumetricKg, 2);
        quote.BillableWeightKg = Math.Round(billableKg, 2);

        // 2. Base Cost Tier by Billable Weight
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

        quote.BaseCost = Money.From(baseCostAmount);
        breakdowns.Add(new QuoteComponentBreakdown("BaseCost", "Base rate per billable weight", baseCostAmount, 0m, baseRule));

        decimal runningSubtotal = baseCostAmount;

        // 3. Distance Surcharge
        decimal distanceKm = distance.Kilometers;
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

        decimal distanceSurchargeAmount = runningSubtotal * distancePct;
        quote.DistanceSurcharge = Money.From(distanceSurchargeAmount);
        if (distanceSurchargeAmount > 0)
        {
            breakdowns.Add(new QuoteComponentBreakdown("DistanceSurcharge", "Distance surcharge", distanceSurchargeAmount, distancePct * 100m, distanceRule));
        }

        // 4. Commercial Value Surcharge
        decimal valAmount = commercialValue.Amount;
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

        decimal valSurchargeAmount = runningSubtotal * valPct;
        quote.CommercialValueSurcharge = Money.From(valSurchargeAmount);
        if (valSurchargeAmount > 0)
        {
            breakdowns.Add(new QuoteComponentBreakdown("CommercialValueSurcharge", "Declared commercial value surcharge", valSurchargeAmount, valPct * 100m, valRule));
        }

        // 5. Delivery Type Multiplier/Surcharge
        decimal typePct;
        string typeRule;

        switch (deliveryType)
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

        decimal currentSubtotal = baseCostAmount + distanceSurchargeAmount + valSurchargeAmount;
        decimal typeSurchargeAmount = currentSubtotal * typePct;
        quote.DeliveryTypeSurcharge = Money.From(typeSurchargeAmount);
        if (typeSurchargeAmount > 0)
        {
            breakdowns.Add(new QuoteComponentBreakdown("DeliveryTypeSurcharge", "Delivery type speed multiplier", typeSurchargeAmount, typePct * 100m, typeRule));
        }

        // 6. Delivery Window Surcharge
        decimal windowPct;
        string windowRule;

        switch (timeWindow)
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

        decimal windowSurchargeAmount = currentSubtotal * windowPct;
        quote.TimeWindowSurcharge = Money.From(windowSurchargeAmount);
        if (windowSurchargeAmount > 0)
        {
            breakdowns.Add(new QuoteComponentBreakdown("TimeWindowSurcharge", "Time window delivery surcharge", windowSurchargeAmount, windowPct * 100m, windowRule));
        }

        // 7. Calculate Total
        decimal totalAmount = baseCostAmount + distanceSurchargeAmount + valSurchargeAmount + typeSurchargeAmount + windowSurchargeAmount;
        quote.Total = Money.From(totalAmount);
        quote.BreakdownComponents = breakdowns;

        return quote;
    }
}
