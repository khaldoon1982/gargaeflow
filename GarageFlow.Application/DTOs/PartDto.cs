namespace GarageFlow.Application.DTOs;

public class PartDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PartNumber { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int MaintenanceRecordId { get; set; }
}

public class CreatePartDto
{
    public string Name { get; set; } = string.Empty;
    public string? PartNumber { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public int MaintenanceRecordId { get; set; }
}
