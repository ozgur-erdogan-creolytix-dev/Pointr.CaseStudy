using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pointr.CaseStudy.Domain.Entities;

namespace Pointr.CaseStudy.Infrastructure.Persistence.Database.Configurations;

public sealed class PagePublicationConfiguration : IEntityTypeConfiguration<PagePublication>
{
    public void Configure(EntityTypeBuilder<PagePublication> builder)
    {
        builder.ToTable("PagePublications");
        builder.HasKey(publication => publication.Id);
        builder.Property(publication => publication.Id).ValueGeneratedNever();

        builder.Property(publication => publication.PageId).IsRequired();
        builder.Property(publication => publication.DraftId).IsRequired();
        builder
            .Property(publication => publication.PublishedUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        // Index to ensure idempotency and enforce unique (PageId, DraftId) pairs
        builder
            .HasIndex(publication => new { publication.PageId, publication.DraftId })
            .IsUnique();

        // Relationship: PagePublication → Page (many publications per page)
        builder
            .HasOne<Page>()
            .WithMany()
            .HasForeignKey(publication => publication.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: PagePublication → PageDraft (many publications per draft)
        builder
            .HasOne<PageDraft>()
            .WithMany()
            .HasForeignKey(publication => publication.DraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
