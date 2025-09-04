using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pointr.Base.Domain.Entities;

namespace Pointr.CaseStudy.Domain.Entities;

public sealed class PageDraft : Entity<Guid>
{
    public Guid PageId { get; private set; }
    public int DraftNumber { get; private set; }
    public string Content { get; private set; } = string.Empty;

    private PageDraft() { }

    public PageDraft(Guid id, Guid pageId, int draftNumber, string content)
        : base(id)
    {
        PageId = pageId;
        DraftNumber = draftNumber;
        Content = content ?? string.Empty;
    }
}
