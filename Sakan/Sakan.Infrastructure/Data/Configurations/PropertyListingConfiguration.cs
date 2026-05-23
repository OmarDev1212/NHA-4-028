using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class PropertyListingConfiguration : IEntityTypeConfiguration<PropertyListing>
    {
        public void Configure(EntityTypeBuilder<PropertyListing> entity)
        {
            entity.HasIndex(e => new { e.ListingType, e.Status, e.Price }, "IX_Listings_Search");

            entity.HasIndex(e => e.PropertyId, "UIX_Listings_OneActivePerProperty")
                .IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("USD")
                .IsFixedLength();
            entity.Property(e => e.ListingType)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(d => d.ListedBy).WithMany()
                .HasForeignKey(d => d.ListedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Listings_ListedBy");

            entity.HasOne(d => d.Property).WithOne(p => p.PropertyListing)
                .HasForeignKey<PropertyListing>(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Listings_Property");

            entity.ToTable("PropertyListings");
        }

    }
}
