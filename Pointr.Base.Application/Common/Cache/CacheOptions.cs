using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Application.Common.Cache;

public sealed class CacheOptions
{
    public TimeSpan? AbsoluteTtl { get; init; } // if value exists, cache for this duration regardless of hits
    public TimeSpan? NegativeTtl { get; init; } // if value does not exist, cache this "not found" state for this duration
}
