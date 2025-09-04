using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pointr.CaseStudy.Domain.Entities;

namespace Pointr.CaseStudy.Application.Interfaces;

public interface IPageCache
{
    Task<Page?> GetPublishedPageAsync(Guid siteId, string slug, CancellationToken ct);
    Task Invalidate(Guid siteId, string slug);
}
