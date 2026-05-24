using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class OwnershipRecordConfiguration : IEntityTypeConfiguration<OwnershipRecord>
    {
        public void Configure(EntityTypeBuilder<OwnershipRecord> entity)
        {
            entity.HasIndex(e => e.PropertyId, "UIX_OR_OneActivePerProperty")
                .IsUnique()
                .HasFilter("([IsActive]=(1))");

            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.AcquiredAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.AcquisitionMethod)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.DeedDocument).WithMany()
                .HasForeignKey(d => d.DeedDocumentId)
                .HasConstraintName("FK_OR_DeedDocument");

            entity.HasOne(d => d.Owner).WithMany()
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OR_Owner");

            entity.ToTable("OwnershipRecords");

        }

    }
}
