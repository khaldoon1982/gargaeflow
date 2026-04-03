using GarageFlow.Domain.Common;
using GarageFlow.Domain.Enums;

namespace GarageFlow.Domain.Entities;

public class Customer : BaseEntity, ISoftDeletable, ISyncable
{
    public int Id { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; } = CustomerType.Private;

    // Private fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Business fields
    public string? CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Department { get; set; }
    public string? BillingEmail { get; set; }
    public string? ChamberOfCommerceNumber { get; set; }
    public string? VATNumber { get; set; }
    public PaymentTerm? PaymentTerm { get; set; }
    public string? DebtorNumber { get; set; }
    public string? FleetManager { get; set; }
    public string? BillingNotes { get; set; }

    // Contact
    public string PhoneNumber { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
    public string? Email { get; set; }
    public PreferredContactMethod PreferredContactMethod { get; set; } = PreferredContactMethod.Phone;

    // Address
    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public string? HouseNumberAddition { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "Nederland";

    // Meta
    public string? Notes { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Sync
    public Guid CloudId { get; set; } = Guid.NewGuid();
    public SyncStatus SyncStatus { get; set; } = SyncStatus.PendingUpload;
    public DateTime? LastSyncedAtUtc { get; set; }
    public DateTime? LastLocalChangeAtUtc { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string? DeviceId { get; set; }

    public string DisplayName => CustomerType == CustomerType.Business
        ? CompanyName ?? string.Empty
        : $"{FirstName} {LastName}".Trim();

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
