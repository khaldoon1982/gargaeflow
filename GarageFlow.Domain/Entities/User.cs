using GarageFlow.Domain.Common;
using GarageFlow.Domain.Enums;

namespace GarageFlow.Domain.Entities;

public class User : BaseEntity, ISoftDeletable
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
