using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pointr.Base.Domain.Entities;

namespace Pointr.CaseStudy.Domain.Entities;

public sealed class Page : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public string Slug { get; private set; } = string.Empty;

    public bool IsArchived { get; private set; }
    public DateTime UpdatedUtc { get; private set; }

    // Concurrency token
    [Timestamp]
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private Page() { }

    public Page(Guid id, Guid siteId, string slug, DateTime updatedUtc)
        : base(id)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("siteId is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("slug is required", nameof(slug));
        if (updatedUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("updatedUtc must be UTC", nameof(updatedUtc));

        SiteId = siteId;
        Slug = slug.Trim();
        UpdatedUtc = updatedUtc;
    }

    public void Archive(DateTime utcNow)
    {
        if (IsArchived)
            return;
        IsArchived = true;
        UpdatedUtc = utcNow; // UTC
    }
}
