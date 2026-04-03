using GarageFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CustomerNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.FirstName).HasMaxLength(100);
        builder.Property(c => c.LastName).HasMaxLength(100);
        builder.Property(c => c.CompanyName).HasMaxLength(200);
        builder.Property(c => c.ContactPerson).HasMaxLength(200);
        builder.Property(c => c.Department).HasMaxLength(100);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.WhatsAppNumber).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.BillingEmail).HasMaxLength(200);
        builder.Property(c => c.ChamberOfCommerceNumber).HasMaxLength(20);
        builder.Property(c => c.VATNumber).HasMaxLength(20);
        builder.Property(c => c.DebtorNumber).HasMaxLength(20);
        builder.Property(c => c.FleetManager).HasMaxLength(200);
        builder.Property(c => c.BillingNotes).HasMaxLength(1000);
        builder.Property(c => c.Street).HasMaxLength(200);
        builder.Property(c => c.HouseNumber).HasMaxLength(10);
        builder.Property(c => c.HouseNumberAddition).HasMaxLength(10);
        builder.Property(c => c.PostalCode).HasMaxLength(10);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Country).HasMaxLength(100);
        builder.Property(c => c.Notes).HasMaxLength(2000);
        builder.HasIndex(c => c.CustomerNumber).IsUnique();
        builder.HasIndex(c => c.PhoneNumber);
        builder.Ignore(c => c.DisplayName);
        builder.ConfigureSyncFields();
        builder.HasQueryFilter(c => c.IsActive);
        builder.HasMany(c => c.Vehicles).WithOne(v => v.Customer).HasForeignKey(v => v.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(c => c.Reminders).WithOne(r => r.Customer).HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
