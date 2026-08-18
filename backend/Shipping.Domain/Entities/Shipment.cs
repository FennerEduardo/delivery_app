using Shipping.Domain.Enums;
using Shipping.Domain.Models;
using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Entities;

public class Shipment
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Address Origin { get; private set; } = null!;
    public Address Destination { get; private set; } = null!;
    public Weight Weight { get; private set; } = null!;
    public Dimensions Dimensions { get; private set; } = null!;
    public Money CommercialValue { get; private set; } = null!;
    public Distance Distance { get; private set; } = null!;
    public DeliveryType DeliveryType { get; private set; }
    public DeliveryWindowType DeliveryWindow { get; private set; }
    public ShipmentStatus Status { get; private set; }

    public Money BaseCost { get; private set; } = Money.Zero();
    public Money TotalCost { get; private set; } = Money.Zero();
    public ShippingQuote? QuoteDetails { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>(); // Optimistic Concurrency Token
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ShipmentStatusHistory> _statusHistory = new();
    public IReadOnlyCollection<ShipmentStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private Shipment() { } // EF Core

    public static Shipment Create(
        Guid customerId,
        Address origin,
        Address destination,
        Weight weight,
        Dimensions dimensions,
        Money commercialValue,
        Distance distance,
        DeliveryType deliveryType,
        DeliveryWindowType deliveryWindow)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required.", nameof(customerId));

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Origin = origin,
            Destination = destination,
            Weight = weight,
            Dimensions = dimensions,
            CommercialValue = commercialValue,
            Distance = distance,
            DeliveryType = deliveryType,
            DeliveryWindow = deliveryWindow,
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        shipment._statusHistory.Add(ShipmentStatusHistory.Create(shipment.Id, ShipmentStatus.Created, ShipmentStatus.Created, "Shipment record created"));
        return shipment;
    }

    public void ApplyQuote(ShippingQuote quote)
    {
        if (Status != ShipmentStatus.Created && Status != ShipmentStatus.Quoted)
            throw new InvalidOperationException($"Cannot apply quote to shipment in state '{Status}'.");

        QuoteDetails = quote ?? throw new ArgumentNullException(nameof(quote));
        BaseCost = quote.BaseCost;
        TotalCost = quote.Total;

        if (Status == ShipmentStatus.Created)
        {
            TransitionStatus(ShipmentStatus.Quoted, "Shipment quote calculated");
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != ShipmentStatus.Quoted)
            throw new InvalidOperationException($"Cannot confirm shipment in state '{Status}'. Must be in 'Quoted' state.");

        TransitionStatus(ShipmentStatus.Confirmed, "Shipment confirmed by customer");
    }

    public void MarkInTransit()
    {
        if (Status != ShipmentStatus.Confirmed)
            throw new InvalidOperationException($"Cannot mark shipment in transit from state '{Status}'. Must be 'Confirmed'.");

        TransitionStatus(ShipmentStatus.InTransit, "Shipment picked up and in transit");
    }

    public void MarkDelivered()
    {
        if (Status != ShipmentStatus.InTransit)
            throw new InvalidOperationException($"Cannot mark shipment delivered from state '{Status}'. Must be 'InTransit'.");

        TransitionStatus(ShipmentStatus.Delivered, "Shipment delivered to destination");
    }

    public void Cancel(string reason)
    {
        if (Status == ShipmentStatus.Delivered || Status == ShipmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel shipment in state '{Status}'.");

        TransitionStatus(ShipmentStatus.Cancelled, string.IsNullOrWhiteSpace(reason) ? "Shipment cancelled" : $"Cancelled: {reason}");
    }

    private void TransitionStatus(ShipmentStatus newStatus, string comment)
    {
        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        _statusHistory.Add(ShipmentStatusHistory.Create(Id, oldStatus, newStatus, comment));
    }
}
