using GarageFlow.Domain.Enums;

namespace GarageFlow.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Department { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
    public string? Email { get; set; }
    public string? BillingEmail { get; set; }
    public PreferredContactMethod PreferredContactMethod { get; set; }
    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public string? HouseNumberAddition { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ChamberOfCommerceNumber { get; set; }
    public string? VATNumber { get; set; }
    public PaymentTerm? PaymentTerm { get; set; }
    public string? DebtorNumber { get; set; }
    public string? FleetManager { get; set; }
    public string? BillingNotes { get; set; }
    public string? Notes { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public int VehicleCount { get; set; }
}

public class CreateCustomerDto
{
    public CustomerType CustomerType { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Department { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
    public string? Email { get; set; }
    public string? BillingEmail { get; set; }
    public PreferredContactMethod PreferredContactMethod { get; set; }
    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public string? HouseNumberAddition { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "Nederland";
    public string? ChamberOfCommerceNumber { get; set; }
    public string? VATNumber { get; set; }
    public PaymentTerm? PaymentTerm { get; set; }
    public string? DebtorNumber { get; set; }
    public string? FleetManager { get; set; }
    public string? BillingNotes { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCustomerDto : CreateCustomerDto
{
    public int Id { get; set; }
    public bool IsArchived { get; set; }
}
