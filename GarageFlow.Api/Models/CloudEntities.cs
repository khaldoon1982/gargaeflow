namespace GarageFlow.Api.Models;

public abstract class CloudEntityBase
{
    public int Id { get; set; }
    public Guid CloudId { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string DeviceId { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public string JsonData { get; set; } = "{}";
}

public class CloudCustomer : CloudEntityBase { }
public class CloudVehicle : CloudEntityBase
{
    public Guid CustomerCloudId { get; set; }
}
public class CloudMaintenanceRecord : CloudEntityBase
{
    public Guid VehicleCloudId { get; set; }
}
public class CloudInspection : CloudEntityBase
{
    public Guid VehicleCloudId { get; set; }
}
public class CloudReminder : CloudEntityBase
{
    public Guid CustomerCloudId { get; set; }
    public Guid? VehicleCloudId { get; set; }
}

public class DeviceRegistration
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;
}

public class SyncLog
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public int EntityCount { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}
