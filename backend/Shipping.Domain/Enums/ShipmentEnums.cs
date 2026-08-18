namespace Shipping.Domain.Enums;

public enum DeliveryType
{
    Standard = 0,
    Express = 1,
    SameDay = 2
}

public enum DeliveryWindowType
{
    Standard = 0,
    Extended = 1,
    Night = 2,
    Weekend = 3
}

public enum ShipmentStatus
{
    Created = 0,
    Quoted = 1,
    Confirmed = 2,
    InTransit = 3,
    Delivered = 4,
    Cancelled = 5
}
