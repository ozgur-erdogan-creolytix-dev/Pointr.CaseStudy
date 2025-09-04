using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Application.Common.Cache;

public readonly record struct CacheKey(string Namespace, string Id)
{
    public override string ToString()
    {
        return $"{Namespace}:{Id}";
    }
}
