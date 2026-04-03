using FluentValidation;
using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Validators;

public class CreateMaintenanceRecordValidator : AbstractValidator<CreateMaintenanceRecordDto>
{
    public CreateMaintenanceRecordValidator()
    {
        RuleFor(x => x.ServiceType).NotEmpty().WithMessage("Type onderhoud is verplicht.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Beschrijving is verplicht.");
        RuleFor(x => x.ServiceDate).NotEmpty().WithMessage("Datum is verplicht.");
        RuleFor(x => x.LaborCost).GreaterThanOrEqualTo(0).WithMessage("Arbeidskosten kunnen niet negatief zijn.");
        RuleFor(x => x.PartsCost).GreaterThanOrEqualTo(0).WithMessage("Onderdeelkosten kunnen niet negatief zijn.");
        RuleFor(x => x.VehicleId).GreaterThan(0).WithMessage("Voertuig is verplicht.");
    }
}
