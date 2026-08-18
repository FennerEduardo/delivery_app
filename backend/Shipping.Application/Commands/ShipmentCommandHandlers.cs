using MediatR;
using Shipping.Application.DTOs;
using Shipping.Application.Interfaces;
using Shipping.Domain.Entities;
using Shipping.Domain.Enums;
using Shipping.Domain.Services.Pricing;
using Shipping.Domain.ValueObjects;

namespace Shipping.Application.Commands;

public class ShipmentCommandHandlers :
    IRequestHandler<CreateShipmentCommand, ShipmentDto>,
    IRequestHandler<GenerateShipmentQuoteCommand, ShippingQuoteDto>,
    IRequestHandler<ConfirmShipmentCommand, bool>,
    IRequestHandler<CancelShipmentCommand, bool>,
    IRequestHandler<ChangeShipmentStatusCommand, bool>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IShippingCostCalculator _calculator;

    public ShipmentCommandHandlers(
        IShipmentRepository shipmentRepository,
        ICustomerRepository customerRepository,
        IShippingCostCalculator calculator)
    {
        _shipmentRepository = shipmentRepository;
        _customerRepository = customerRepository;
        _calculator = calculator;
    }

    public async Task<ShipmentDto> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var customerExists = await _customerRepository.ExistsAsync(request.CustomerId, cancellationToken);
        if (!customerExists)
        {
            throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' was not found.");
        }

        var origin = Address.Create(request.Origin.Street, request.Origin.City, request.Origin.State, request.Origin.ZipCode, request.Origin.Country);
        var dest = Address.Create(request.Destination.Street, request.Destination.City, request.Destination.State, request.Destination.ZipCode, request.Destination.Country);
        var weight = Weight.FromKg(request.WeightKg);
        var dims = Dimensions.Create(request.LengthCm, request.WidthCm, request.HeightCm);
        var value = Money.From(request.CommercialValue);
        var dist = Distance.FromKm(request.DistanceKm);

        var shipment = Shipment.Create(request.CustomerId, origin, dest, weight, dims, value, dist, request.DeliveryType, request.DeliveryWindow);

        // Auto-quote upon creation
        var quote = _calculator.CalculateQuote(weight, dims, value, dist, request.DeliveryType, request.DeliveryWindow);
        shipment.ApplyQuote(quote);

        await _shipmentRepository.AddAsync(shipment, cancellationToken);

        return MapToDto(shipment);
    }

    public async Task<ShippingQuoteDto> Handle(GenerateShipmentQuoteCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Shipment '{request.ShipmentId}' not found.");

        var quote = _calculator.CalculateQuote(shipment.Weight, shipment.Dimensions, shipment.CommercialValue, shipment.Distance, shipment.DeliveryType, shipment.DeliveryWindow);
        shipment.ApplyQuote(quote);
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);

        return MapQuoteDto(quote);
    }

    public async Task<bool> Handle(ConfirmShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Shipment '{request.ShipmentId}' not found.");

        shipment.Confirm();
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);
        return true;
    }

    public async Task<bool> Handle(CancelShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Shipment '{request.ShipmentId}' not found.");

        shipment.Cancel(request.Reason);
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);
        return true;
    }

    public async Task<bool> Handle(ChangeShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Shipment '{request.ShipmentId}' not found.");

        switch (request.NewStatus)
        {
            case ShipmentStatus.Confirmed:
                shipment.Confirm();
                break;
            case ShipmentStatus.InTransit:
                shipment.MarkInTransit();
                break;
            case ShipmentStatus.Delivered:
                shipment.MarkDelivered();
                break;
            case ShipmentStatus.Cancelled:
                shipment.Cancel(request.Comment);
                break;
            default:
                throw new InvalidOperationException($"Invalid status transition to '{request.NewStatus}'.");
        }

        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);
        return true;
    }

    private static ShipmentDto MapToDto(Shipment s)
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
