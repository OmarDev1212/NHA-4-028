using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class OwnershipTransferConfiguration : IEntityTypeConfiguration<OwnershipTransfer>
    {
        public void Configure(EntityTypeBuilder<OwnershipTransfer> entity)
        {
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.AgreedPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CancellationReason).HasMaxLength(1000);
            entity.Property(e => e.InitiatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Initiated");

            entity.HasOne(d => d.Agent).WithMany()
                .HasForeignKey(d => d.AgentId)
                .HasConstraintName("FK_OT_Agent");

            entity.HasOne(d => d.EscrowPayment).WithMany()
                .HasForeignKey(d => d.EscrowPaymentId)
                .HasConstraintName("FK_OT_EscrowPayment");

            entity.HasOne(d => d.FromOwner).WithMany()
                .HasForeignKey(d => d.FromOwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OT_FromOwner");

            entity.HasOne(d => d.Property).WithMany(p => p.OwnershipTransfers)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OT_Property");

            entity.HasOne(d => d.ToOwner).WithMany()
                .HasForeignKey(d => d.ToOwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OT_ToOwner");
            entity.ToTable("OwnershipTransfers");

        }

    }
}
