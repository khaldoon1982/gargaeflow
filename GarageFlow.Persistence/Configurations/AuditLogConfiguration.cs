using GarageFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ActionType).IsRequired().HasMaxLength(50);
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Details).HasMaxLength(2000);
        builder.HasOne(a => a.User).WithMany(u => u.AuditLogs).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
