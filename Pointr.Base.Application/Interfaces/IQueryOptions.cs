using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Application.Interfaces;

public interface IQueryOptions<T>
{
    Expression<Func<T, bool>>? Filter { get; }
    List<Expression<Func<T, object>>>? Includes { get; }
    List<string>? IncludeStrings { get; }

    Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; }
    Func<IQueryable<T>, IOrderedQueryable<T>>? OrderByDescending { get; }

    int? Skip { get; }
    int? Take { get; }
    bool AsNoTracking { get; }
}
