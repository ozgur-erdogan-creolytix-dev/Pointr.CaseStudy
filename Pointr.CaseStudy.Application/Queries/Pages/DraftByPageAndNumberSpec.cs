using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Pointr.Base.Application.Interfaces;
using Pointr.CaseStudy.Application.Interfaces;
using Pointr.CaseStudy.Domain.Entities;

namespace Pointr.CaseStudy.Application.Queries.Pages;

public sealed class DraftByPageAndNumberSpec : IQueryOptions<PageDraft>
{
    public Expression<Func<PageDraft, bool>>? Filter { get; }
    public List<Expression<Func<PageDraft, object>>>? Includes => null;
    public List<string>? IncludeStrings => null;
    public Func<IQueryable<PageDraft>, IOrderedQueryable<PageDraft>>? OrderBy => null;
    public Func<IQueryable<PageDraft>, IOrderedQueryable<PageDraft>>? OrderByDescending => null;
    public int? Skip => null;
    public int? Take => null;
    public bool AsNoTracking => true;

    public DraftByPageAndNumberSpec(Guid pageId, int draftNo)
    {
        Filter = d => d.PageId == pageId && d.DraftNumber == draftNo;
    }
}
