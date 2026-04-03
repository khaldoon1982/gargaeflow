using GarageFlow.Domain.Common;

namespace GarageFlow.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Details { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }
}
