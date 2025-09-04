using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pointr.Base.Application.Interfaces;
using Pointr.Base.Infrastructure.Caching.Memory;
using Pointr.Base.Infrastructure.Time;

namespace Pointr.Base.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBaseInfrastructure(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheClient, MemoryCacheClient>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}
