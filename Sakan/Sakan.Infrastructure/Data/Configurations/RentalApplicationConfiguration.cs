using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class RentalApplicationConfiguration : IEntityTypeConfiguration<RentalApplication>
    {
        public void Configure(EntityTypeBuilder<RentalApplication> entity)
        {
            entity.HasIndex(e => new { e.ListingId, e.Status, e.RequestedFrom, e.RequestedTo }, "IX_RA_ListingDates");

            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.MonthlyBudget).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(d => d.Applicant).WithMany()
                .HasForeignKey(d => d.ApplicantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RA_Applicant");

            entity.HasOne(d => d.ContractDocument).WithMany()
                .HasForeignKey(d => d.ContractDocumentId)
                .HasConstraintName("FK_RA_ContractDocument");

            entity.HasOne(d => d.Listing).WithMany()
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RA_Listing");
            entity.ToTable("RentalApplications");

        }

    }
}
