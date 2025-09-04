using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pointr.CaseStudy.Domain.Entities;

namespace Pointr.CaseStudy.Infrastructure.Persistence.Database.Configurations;

public sealed class PageDraftConfiguration : IEntityTypeConfiguration<PageDraft>
{
    public void Configure(EntityTypeBuilder<PageDraft> builder)
    {
        builder.ToTable("PageDrafts");
        builder.HasKey(draft => draft.Id);
        builder.Property(draft => draft.Id).ValueGeneratedNever();

        builder.Property(draft => draft.PageId).IsRequired();
        builder.Property(draft => draft.DraftNumber).IsRequired();
        builder.Property(draft => draft.Content).IsRequired();

        // Ensure each draft number is unique per page
        builder.HasIndex(draft => new { draft.PageId, draft.DraftNumber }).IsUnique();

        // Relationship: PageDraft → Page (no navigation property, so WithMany() is empty)
        builder
            .HasOne<Page>()
            .WithMany()
            .HasForeignKey(draft => draft.PageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
