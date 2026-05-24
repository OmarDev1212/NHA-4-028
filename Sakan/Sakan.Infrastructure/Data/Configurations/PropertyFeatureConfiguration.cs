using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sakan.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Sakan.Infrastructure.Data.Configurations
{
    public class PropertyFeatureConfiguration : IEntityTypeConfiguration<PropertyFeature>
    {
        public void Configure(EntityTypeBuilder<PropertyFeature> entity)
        {
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Feature)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyFeatures)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("FK_PropertyFeatures_Property");

            entity.ToTable("PropertyFeatures");


        }

    }
}
