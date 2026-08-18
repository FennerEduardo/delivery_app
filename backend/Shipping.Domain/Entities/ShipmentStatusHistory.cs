using Shipping.Domain.Enums;

namespace Shipping.Domain.Entities;

public class ShipmentStatusHistory
{
    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public ShipmentStatus PreviousStatus { get; private set; }
    public ShipmentStatus NewStatus { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTime ChangedAt { get; private set; }

    private ShipmentStatusHistory() { } // EF Core

    public static ShipmentStatusHistory Create(Guid shipmentId, ShipmentStatus previousStatus, ShipmentStatus newStatus, string comment = "")
    {
        return new ShipmentStatusHistory
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Comment = comment,
            ChangedAt = DateTime.UtcNow
        };
    }
}
