using MediatR;
using Shipping.Application.DTOs;
using Shipping.Application.Interfaces;
using Shipping.Domain.Entities;
using Shipping.Domain.Enums;
using Shipping.Domain.Services.Pricing;
using Shipping.Domain.ValueObjects;

namespace Shipping.Application.Commands;

public record CreateCustomerCommand(string Name, string Email, string Phone, AddressDto Address) : IRequest<CustomerDto>;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;

    public CreateCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var address = Address.Create(request.Address.Street, request.Address.City, request.Address.State, request.Address.ZipCode, request.Address.Country);
        var customer = Customer.Create(request.Name, request.Email, request.Phone, address);

        await _customerRepository.AddAsync(customer, cancellationToken);

        return new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Phone,
            new AddressDto(address.Street, address.City, address.State, address.ZipCode, address.Country), customer.CreatedAt);
    }
}

public record CreateShipmentCommand(
    Guid CustomerId,
    AddressDto Origin,
    AddressDto Destination,
    decimal WeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    decimal CommercialValue,
    decimal DistanceKm,
    DeliveryType DeliveryType,
    DeliveryWindowType DeliveryWindow
) : IRequest<ShipmentDto>;

public record GenerateShipmentQuoteCommand(Guid ShipmentId) : IRequest<ShippingQuoteDto>;

public record ConfirmShipmentCommand(Guid ShipmentId) : IRequest<bool>;

public record CancelShipmentCommand(Guid ShipmentId, string Reason) : IRequest<bool>;

public record ChangeShipmentStatusCommand(Guid ShipmentId, ShipmentStatus NewStatus, string Comment) : IRequest<bool>;
