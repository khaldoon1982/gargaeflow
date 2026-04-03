using FluentValidation.TestHelper;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Validators;
using GarageFlow.Domain.Enums;
using Xunit;

namespace GarageFlow.Tests.Validators;

public class CreateVehicleValidatorTests
{
    private readonly CreateVehicleValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenPlateNumberEmpty()
    {
        var dto = new CreateVehicleDto { PlateNumber = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PlateNumber);
    }

    [Fact]
    public void ShouldHaveError_WhenYearInvalid()
    {
        var dto = new CreateVehicleDto { Year = 1800 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Fact]
    public void ShouldHaveError_WhenCustomerIdZero()
    {
        var dto = new CreateVehicleDto { CustomerId = 0 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAllFieldsValid()
    {
        var dto = new CreateVehicleDto
        {
            PlateNumber = "AB-123-CD",
            Brand = "VW",
            Model = "Golf",
            Year = 2020,
            FuelType = FuelType.Benzine,
            TransmissionType = TransmissionType.Handgeschakeld,
            Mileage = 10000,
            CustomerId = 1
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
