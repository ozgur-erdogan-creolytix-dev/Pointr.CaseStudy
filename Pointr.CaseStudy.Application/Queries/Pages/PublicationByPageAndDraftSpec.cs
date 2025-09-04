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

public sealed class PublicationByPageAndDraftSpec : IQueryOptions<PagePublication>
{
    public Expression<Func<PagePublication, bool>>? Filter { get; }
    public List<Expression<Func<PagePublication, object>>>? Includes => null;
    public List<string>? IncludeStrings => null;
    public Func<IQueryable<PagePublication>, IOrderedQueryable<PagePublication>>? OrderBy => null;
    public Func<
        IQueryable<PagePublication>,
        IOrderedQueryable<PagePublication>
    >? OrderByDescending => null;
    public int? Skip => null;
    public int? Take => null;
    public bool AsNoTracking => true;

    public PublicationByPageAndDraftSpec(Guid pageId, Guid draftId)
    {
        Filter = p => p.PageId == pageId && p.DraftId == draftId;
    }
}
