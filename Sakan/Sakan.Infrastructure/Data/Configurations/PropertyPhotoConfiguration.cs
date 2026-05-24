using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class PropertyPhotoConfiguration : IEntityTypeConfiguration<PropertyPhoto>
    {
        public void Configure(EntityTypeBuilder<PropertyPhoto> entity)
        {
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.PhotoUrl)
                .IsRequired()   
                .HasMaxLength(1000);

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyPhotos)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("FK_PropertyPhotos_Property");
            entity.ToTable("PropertyPhotos");
        }

    }
}
