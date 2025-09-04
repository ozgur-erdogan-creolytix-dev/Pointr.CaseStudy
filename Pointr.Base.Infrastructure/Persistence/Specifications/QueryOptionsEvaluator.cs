using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pointr.Base.Application.Interfaces;

namespace Pointr.Base.Infrastructure.Persistence.Specifications;

public static class QueryOptionsEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> source, IQueryOptions<T> queryOptions)
        where T : class
    {
        IQueryable<T> query = source;

        // 1) Filter (WHERE)
        if (queryOptions.Filter is not null)
            query = query.Where(queryOptions.Filter);

        // 2) Includes via expression paths
        if (queryOptions.Includes is not null)
        {
            foreach (var includeExpression in queryOptions.Includes)
                query = query.Include(includeExpression);
        }

        // 3) Includes via string paths
        if (queryOptions.IncludeStrings is not null)
        {
            foreach (var includePath in queryOptions.IncludeStrings)
                query = query.Include(includePath);
        }

        // 4) Ordering (prefer OrderBy if both provided)
        if (queryOptions.OrderBy is not null)
            query = queryOptions.OrderBy(query);
        else if (queryOptions.OrderByDescending is not null)
            query = queryOptions.OrderByDescending(query);

        // 5) Pagination
        if (queryOptions.Skip.HasValue)
            query = query.Skip(queryOptions.Skip.Value);

        if (queryOptions.Take.HasValue)
            query = query.Take(queryOptions.Take.Value);

        // 6) Tracking behavior
        if (queryOptions.AsNoTracking)
            query = query.AsNoTracking();

        return query;
    }
}
