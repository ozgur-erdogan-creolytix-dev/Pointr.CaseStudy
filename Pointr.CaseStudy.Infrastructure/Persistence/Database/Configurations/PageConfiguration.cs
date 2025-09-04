using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pointr.CaseStudy.Domain.Entities;

namespace Pointr.CaseStudy.Infrastructure.Persistence.Database.Configurations;

public sealed class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("Pages");

        builder.HasKey(page => page.Id);
        builder.Property(page => page.Id).ValueGeneratedNever();

        builder.Property(page => page.SiteId).IsRequired();
        builder.Property(page => page.Slug).IsRequired().HasMaxLength(256);

        // Ensure each slug is unique within the same site
        builder.HasIndex(page => new { page.SiteId, page.Slug }).IsUnique();

        builder.Property(page => page.IsArchived).IsRequired();
        builder.Property(page => page.UpdatedUtc).IsRequired();

        // Concurrency token setup (RowVersion stored as PostgreSQL 'bytea')
        builder
            .Property(page => page.RowVersion)
            .HasColumnName("RowVersion")
            .HasColumnType("bytea")
            .IsRequired()
            .IsConcurrencyToken();

        // Explicit UTC datetime type mapping
        builder.Property(page => page.UpdatedUtc).HasColumnType("timestamp with time zone");
    }
}
