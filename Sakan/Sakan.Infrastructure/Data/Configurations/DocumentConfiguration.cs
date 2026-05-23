using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> entity)
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.DocumentType)
                .IsRequired()
                .HasMaxLength(25);
            entity.Property(e => e.FileHash)
                .IsRequired()
                .HasMaxLength(64)
                .IsFixedLength();
            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.RelatedEntityType)
                .IsRequired()
                .HasMaxLength(30);
            entity.Property(e => e.StorageUrl)
                .IsRequired()
                .HasMaxLength(2000);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(sysdatetimeoffset())");

            entity.HasOne(d => d.UploadedBy).WithMany()
                .HasForeignKey(d => d.UploadedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Doc_UploadedBy");
            entity.ToTable("Documents");

        }

    }
}
