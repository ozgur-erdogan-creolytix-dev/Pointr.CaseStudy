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

public sealed class PageBySiteAndSlugSpec : IQueryOptions<Page>
{
    public Expression<Func<Page, bool>>? Filter { get; }
    public List<Expression<Func<Page, object>>>? Includes => null;
    public List<string>? IncludeStrings => null;
    public Func<IQueryable<Page>, IOrderedQueryable<Page>>? OrderBy => null;
    public Func<IQueryable<Page>, IOrderedQueryable<Page>>? OrderByDescending => null;
    public int? Skip => null;
    public int? Take => null;
    public bool AsNoTracking => false;

    public PageBySiteAndSlugSpec(Guid siteId, string slug)
    {
        Filter = p => p.SiteId == siteId && p.Slug == slug;
    }
}
