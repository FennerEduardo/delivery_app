using MediatR;
using Shipping.Application.DTOs;
using Shipping.Application.Interfaces;

namespace Shipping.Application.Queries;

public record GetShipmentByIdQuery(Guid Id) : IRequest<ShipmentDto?>;

public record GetShipmentsQuery(int Skip = 0, int Take = 50) : IRequest<IReadOnlyList<ShipmentDto>>;

public record GetShipmentHistoryQuery(Guid ShipmentId) : IRequest<IReadOnlyList<ShipmentStatusHistoryDto>>;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;

public class QueryHandlers :
    IRequestHandler<GetShipmentByIdQuery, ShipmentDto?>,
    IRequestHandler<GetShipmentsQuery, IReadOnlyList<ShipmentDto>>,
    IRequestHandler<GetShipmentHistoryQuery, IReadOnlyList<ShipmentStatusHistoryDto>>,
    IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICustomerRepository _customerRepository;

    public QueryHandlers(IShipmentRepository shipmentRepository, ICustomerRepository customerRepository)
    {
        _shipmentRepository = shipmentRepository;
        _customerRepository = customerRepository;
    }

    public async Task<ShipmentDto?> Handle(GetShipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var s = await _shipmentRepository.GetByIdAsync(request.Id, cancellationToken);
        return s == null ? null : MapToDto(s);
    }

    public async Task<IReadOnlyList<ShipmentDto>> Handle(GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        var shipments = await _shipmentRepository.GetAllAsync(request.Skip, request.Take, cancellationToken);
        return shipments.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ShipmentStatusHistoryDto>> Handle(GetShipmentHistoryQuery request, CancellationToken cancellationToken)
    {
        var s = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Shipment '{request.ShipmentId}' not found.");

        return s.StatusHistory.Select(h => new ShipmentStatusHistoryDto(h.Id, h.ShipmentId, h.PreviousStatus.ToString(), h.NewStatus.ToString(), h.Comment, h.ChangedAt)).ToList();
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (c == null) return null;

        return new CustomerDto(c.Id, c.Name, c.Email, c.Phone,
            new AddressDto(c.Address.Street, c.Address.City, c.Address.State, c.Address.ZipCode, c.Address.Country), c.CreatedAt);
    }

    private static ShipmentDto MapToDto(Domain.Entities.Shipment s)
    {
        return new ShipmentDto(
            s.Id,
            s.CustomerId,
            new AddressDto(s.Origin.Street, s.Origin.City, s.Origin.State, s.Origin.ZipCode, s.Origin.Country),
            new AddressDto(s.Destination.Street, s.Destination.City, s.Destination.State, s.Destination.ZipCode, s.Destination.Country),
            s.Weight.Kilograms,
            s.Dimensions.LengthCm,
            s.Dimensions.WidthCm,
            s.Dimensions.HeightCm,
            s.CommercialValue.Amount,
            s.Distance.Kilometers,
            s.DeliveryType.ToString(),
            s.DeliveryWindow.ToString(),
            s.Status.ToString(),
            s.BaseCost.Amount,
            s.TotalCost.Amount,
            s.QuoteDetails != null ? MapQuoteDto(s.QuoteDetails) : null,
            s.StatusHistory.Select(h => new ShipmentStatusHistoryDto(h.Id, h.ShipmentId, h.PreviousStatus.ToString(), h.NewStatus.ToString(), h.Comment, h.ChangedAt)).ToList(),
            s.CreatedAt,
            s.UpdatedAt
        );
    }

    private static ShippingQuoteDto MapQuoteDto(Domain.Models.ShippingQuote q)
    {
        return new ShippingQuoteDto(
            q.BaseCost.Amount,
            q.WeightSurcharge.Amount,
            q.DistanceSurcharge.Amount,
            q.CommercialValueSurcharge.Amount,
            q.DeliveryTypeSurcharge.Amount,
            q.TimeWindowSurcharge.Amount,
            q.Discount.Amount,
            q.Total.Amount,
            q.ActualWeightKg,
            q.VolumetricWeightKg,
            q.BillableWeightKg,
            q.BreakdownComponents.Select(b => new QuoteBreakdownDto(b.ComponentName, b.Description, b.Amount, b.Percentage, b.RuleApplied)).ToList()
        );
    }
}
