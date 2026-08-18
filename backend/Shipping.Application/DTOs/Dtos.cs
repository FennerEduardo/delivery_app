namespace Shipping.Application.DTOs;

public record AddressDto(string Street, string City, string State, string ZipCode, string Country);

public record CustomerDto(Guid Id, string Name, string Email, string Phone, AddressDto Address, DateTime CreatedAt);

public record QuoteBreakdownDto(string ComponentName, string Description, decimal Amount, decimal Percentage, string RuleApplied);

public record ShippingQuoteDto(
    decimal BaseCost,
    decimal WeightSurcharge,
    decimal DistanceSurcharge,
    decimal CommercialValueSurcharge,
    decimal DeliveryTypeSurcharge,
    decimal TimeWindowSurcharge,
    decimal Discount,
    decimal Total,
    decimal ActualWeightKg,
    decimal VolumetricWeightKg,
    decimal BillableWeightKg,
    List<QuoteBreakdownDto> Breakdown
);

public record ShipmentStatusHistoryDto(Guid Id, Guid ShipmentId, string PreviousStatus, string NewStatus, string Comment, DateTime ChangedAt);

public record ShipmentDto(
    Guid Id,
    Guid CustomerId,
    AddressDto Origin,
    AddressDto Destination,
    decimal WeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    decimal CommercialValue,
    decimal DistanceKm,
    string DeliveryType,
    string DeliveryWindow,
    string Status,
    decimal BaseCost,
    decimal TotalCost,
    ShippingQuoteDto? Quote,
    List<ShipmentStatusHistoryDto> History,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
