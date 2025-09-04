using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pointr.Base.Domain.Entities;

namespace Pointr.CaseStudy.Domain.Entities;

public sealed class PagePublication : Entity<Guid>
{
    public Guid PageId { get; private set; }
    public Guid DraftId { get; private set; }
    public DateTime PublishedUtc { get; private set; }

    private PagePublication() { }

    public PagePublication(Guid id, Guid pageId, Guid draftId, DateTime publishedUtc)
        : base(id)
    {
        PageId = pageId;
        DraftId = draftId;
        PublishedUtc = publishedUtc; // UTC
    }
}
