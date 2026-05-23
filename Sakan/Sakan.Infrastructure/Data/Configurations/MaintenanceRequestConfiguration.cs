using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintanceRequest>
    {
        public void Configure(EntityTypeBuilder<MaintanceRequest> entity)
        {
            entity.HasIndex(e => new { e.Status, e.RequestedAt }, "IX_MR_StatusRequested");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ActualCostUSD)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("ActualCostUSD");
            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000)
                .HasDefaultValue("");
            entity.Property(e => e.EstimatedCostUSD)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("EstimatedCostUSD");
            entity.Property(e => e.Priority)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(15);
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasOne(d => d.AssignedTo).WithMany()
                .HasForeignKey(d => d.AssignedToId)
                .HasConstraintName("FK_MR_AssignedTo");

            entity.HasOne(d => d.Property).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MR_Property");

            entity.HasOne(d => d.RequestedBy).WithMany()
                .HasForeignKey(d => d.RequestedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MR_RequestedBy");
            entity.ToTable("MaintainanceRequests");
        }

    }
}
