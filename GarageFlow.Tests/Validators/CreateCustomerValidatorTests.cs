using FluentValidation.TestHelper;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Validators;
using GarageFlow.Domain.Enums;
using Xunit;

namespace GarageFlow.Tests.Validators;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenPhoneNumberEmpty()
    {
        var dto = new CreateCustomerDto { CustomerType = CustomerType.Private, FirstName = "Test", LastName = "Klant", PhoneNumber = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void ShouldHaveError_WhenPrivateAndFirstNameEmpty()
    {
        var dto = new CreateCustomerDto { CustomerType = CustomerType.Private, FirstName = "", LastName = "Test", PhoneNumber = "06-1" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ShouldHaveError_WhenBusinessAndCompanyNameEmpty()
    {
        var dto = new CreateCustomerDto { CustomerType = CustomerType.Business, CompanyName = "", PhoneNumber = "020-1" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CompanyName);
    }

    [Fact]
    public void ShouldHaveError_WhenEmailInvalid()
    {
        var dto = new CreateCustomerDto { CustomerType = CustomerType.Private, FirstName = "T", LastName = "K", PhoneNumber = "06-1", Email = "invalid" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShouldNotHaveError_WhenPrivateValid()
    {
        var dto = new CreateCustomerDto { CustomerType = CustomerType.Private, FirstName = "Test", LastName = "Klant", PhoneNumber = "06-12345678" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldNotHaveError_WhenBusinessValid()
    {
        var dto = new CreateCustomerDto { CustomerType = CustomerType.Business, CompanyName = "Test B.V.", PhoneNumber = "020-1234567" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
