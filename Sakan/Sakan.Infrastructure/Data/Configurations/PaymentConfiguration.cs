using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> entity)
        {
            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId, e.Status }, "IX_Payments_Reference");

            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("USD")
                .IsFixedLength();
            entity.Property(e => e.GatewayTransactionId).HasMaxLength(200);
            entity.Property(e => e.PaymentType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(25);
            entity.Property(e => e.ReferenceType)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(15);

            entity.HasOne(d => d.Payee).WithMany()
                .HasForeignKey(d => d.PayeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pay_Payee");

            entity.HasOne(d => d.Payer).WithMany()
                .HasForeignKey(d => d.PayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pay_Payer");

            entity.HasOne(d => d.RentalApplication).WithMany(p => p.Payments)
                .HasForeignKey(d => d.RentalApplicationId)
                .HasConstraintName("FK_Payments_RentalApplications_RentalApplicationId");

            entity.ToTable("Payments");

        }

    }
}
