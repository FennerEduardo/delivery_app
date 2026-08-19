using Shipping.Domain.Enums;
using Shipping.Domain.Models;
using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Services.Pricing.Rules;

public record PricingRuleEvaluationContext(
    Weight Weight,
    Dimensions Dimensions,
    Money CommercialValue,
    Distance Distance,
    DeliveryType DeliveryType,
    DeliveryWindowType TimeWindow,
    decimal ActualWeightKg,
    decimal VolumetricWeightKg,
    decimal BillableWeightKg,
    decimal CurrentSubtotal
);

public interface IPricingRule
{
    string RuleId { get; }
    string RuleName { get; }
    int ExecutionOrder { get; }

    QuoteComponentBreakdown Evaluate(PricingRuleEvaluationContext context);
}
