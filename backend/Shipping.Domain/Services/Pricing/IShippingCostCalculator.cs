using Shipping.Domain.Enums;
using Shipping.Domain.Models;
using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Services.Pricing;

public interface IShippingCostCalculator
{
    ShippingQuote CalculateQuote(
        Weight actualWeight,
        Dimensions dimensions,
        Money commercialValue,
        Distance distance,
        DeliveryType deliveryType,
        DeliveryWindowType timeWindow
    );
}
