using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> entity)
        {
            entity.HasIndex(e => new { e.RecipientId, e.IsRead, e.CreatedAt }, "IX_Notifications_Inbox").IsDescending(false, false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Body)
                            .IsRequired()
                            .HasMaxLength(2000);
            entity.Property(e => e.Channel)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("InApp");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.RelatedEntityType).HasMaxLength(30);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasOne(d => d.Recipient).WithMany()
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notif_Recipient");
            entity.ToTable("Notifications");
        }

    }
}
