using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class ViewingAppointmentConfiguration : IEntityTypeConfiguration<ViewingAppoinment>
    {
        public void Configure(EntityTypeBuilder<ViewingAppoinment> entity)
        {
            entity.HasIndex(e => new { e.ListingId, e.Status, e.ScheduledAt }, "IX_VA_ListingStatus");

            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.DurationMinutes).HasDefaultValue(30);
            entity.Property(e => e.MeetingType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.VirtualMeetingUrl).HasMaxLength(1000);

            entity.HasOne(d => d.Agent).WithMany()
                .HasForeignKey(d => d.AgentId)
                .HasConstraintName("FK_VA_Agent");

            entity.HasOne(d => d.Listing).WithMany()
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VA_Listing");

            entity.HasOne(d => d.RequestedBy).WithMany()
                .HasForeignKey(d => d.RequestedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VA_RequestedBy");
            entity.ToTable("ViewingAppoinments");

        }

    }
}
