using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;

namespace Sakan.Infrastructure.Data.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {

        //builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.AreaSqm).HasColumnType("decimal(12, 2)");

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");

        builder.Property(e => e.LegalStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.OwnsOne(p => p.Address, addr =>
        {
            addr.Property(a => a.Street).IsRequired().HasMaxLength(300).HasColumnName("Address_Street");
            addr.Property(a => a.City).IsRequired().HasMaxLength(100).HasColumnName("Address_City");
            addr.Property(a => a.State).IsRequired().HasMaxLength(100).HasColumnName("Address_State");
            addr.Property(a => a.Country).IsRequired().HasMaxLength(100).HasColumnName("Address_Country");
            addr.Property(a => a.PostalCode).IsRequired().HasMaxLength(20).HasColumnName("Address_PostalCode");

        });

        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetimeoffset())");




        builder.HasMany(p => p.OwnershipRecords)
           .WithOne(o => o.Property)
           .HasForeignKey(o => o.PropertyId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.PropertyListing)
            .WithOne(l => l.Property)
            .HasForeignKey<PropertyListing>(l => l.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.MaintenanceRequests)
            .WithOne(m => m.Property)
            .HasForeignKey(m => m.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Documents)
            .WithOne()
            .HasForeignKey(d => d.RelatedEntityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable("Properties");

    }

}
