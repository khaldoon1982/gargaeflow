using FluentValidation;
using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Validators;

public class CreateVehicleValidator : AbstractValidator<CreateVehicleDto>
{
    public CreateVehicleValidator()
    {
        RuleFor(x => x.PlateNumber).NotEmpty().WithMessage("Kenteken is verplicht.").MaximumLength(20);
        RuleFor(x => x.Brand).NotEmpty().WithMessage("Merk is verplicht.").MaximumLength(50);
        RuleFor(x => x.Model).NotEmpty().WithMessage("Model is verplicht.").MaximumLength(50);
        RuleFor(x => x.Year).InclusiveBetween(1900, DateTime.Now.Year + 1).WithMessage("Ongeldig bouwjaar.");
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0).WithMessage("Kilometerstand kan niet negatief zijn.");
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Klant is verplicht.");
        RuleFor(x => x.FuelType).IsInEnum();
        RuleFor(x => x.TransmissionType).IsInEnum();
    }
}
