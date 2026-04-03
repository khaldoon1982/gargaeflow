using FluentValidation;
using GarageFlow.Application.DTOs;
using GarageFlow.Domain.Enums;

namespace GarageFlow.Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Telefoonnummer is verplicht.").MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Ongeldig e-mailadres.");
        RuleFor(x => x.BillingEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.BillingEmail)).WithMessage("Ongeldig factuur e-mailadres.");

        // Private customer rules
        When(x => x.CustomerType == CustomerType.Private, () =>
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("Voornaam is verplicht.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Achternaam is verplicht.");
        });

        // Business customer rules
        When(x => x.CustomerType == CustomerType.Business, () =>
        {
            RuleFor(x => x.CompanyName).NotEmpty().WithMessage("Bedrijfsnaam is verplicht.");
        });
    }
}
