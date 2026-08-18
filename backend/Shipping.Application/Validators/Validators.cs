using FluentValidation;
using Shipping.Application.Commands;

namespace Shipping.Application.Validators;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Customer name is required.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid customer email is required.");
        RuleFor(x => x.Address.Street).NotEmpty().WithMessage("Street address is required.");
        RuleFor(x => x.Address.City).NotEmpty().WithMessage("City is required.");
    }
}

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
        RuleFor(x => x.WeightKg).GreaterThan(0).WithMessage("Weight must be greater than zero.");
        RuleFor(x => x.LengthCm).GreaterThan(0).WithMessage("Length must be greater than zero.");
        RuleFor(x => x.WidthCm).GreaterThan(0).WithMessage("Width must be greater than zero.");
        RuleFor(x => x.HeightCm).GreaterThan(0).WithMessage("Height must be greater than zero.");
        RuleFor(x => x.CommercialValue).GreaterThanOrEqualTo(0).WithMessage("Commercial value cannot be negative.");
        RuleFor(x => x.DistanceKm).GreaterThanOrEqualTo(0).WithMessage("Distance cannot be negative.");
        RuleFor(x => x.Origin.Street).NotEmpty().WithMessage("Origin street is required.");
        RuleFor(x => x.Origin.City).NotEmpty().WithMessage("Origin city is required.");
        RuleFor(x => x.Destination.Street).NotEmpty().WithMessage("Destination street is required.");
        RuleFor(x => x.Destination.City).NotEmpty().WithMessage("Destination city is required.");
    }
}
