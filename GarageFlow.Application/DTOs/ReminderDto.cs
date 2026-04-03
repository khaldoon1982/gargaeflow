using GarageFlow.Domain.Enums;

namespace GarageFlow.Application.DTOs;

public class ReminderDto
{
    public int Id { get; set; }
    public ReminderType ReminderType { get; set; }
    public DateTime ReminderDate { get; set; }
    public string Message { get; set; } = string.Empty;
    public SendMethod SendMethod { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public ReminderStatus Status { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? VehicleId { get; set; }
    public string? VehiclePlate { get; set; }
}

public class CreateReminderDto
{
    public ReminderType ReminderType { get; set; }
    public DateTime ReminderDate { get; set; }
    public string Message { get; set; } = string.Empty;
    public SendMethod SendMethod { get; set; }
    public int CustomerId { get; set; }
    public int? VehicleId { get; set; }
}
