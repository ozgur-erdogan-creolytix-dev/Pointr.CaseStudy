using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pointr.Base.Application.Interfaces;
using Pointr.Base.Infrastructure.Persistence.Repositories;
using Pointr.Base.Infrastructure.Persistence.UnitOfWork;
using Pointr.CaseStudy.Application.Interfaces;
using Pointr.CaseStudy.Infrastructure.Caching;
using Pointr.CaseStudy.Infrastructure.Persistence.Database;

namespace Pointr.CaseStudy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCaseStudyInfrastructure(
        this IServiceCollection services,
        string? connectionString
    )
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IGenericRepository>(
            serviceProvider => new EfGenericRepository<AppDbContext>(
                serviceProvider.GetRequiredService<AppDbContext>()
            )
        );

        services.AddScoped<IGenericRepositoryUnitOfWork>(
            serviceProvider => new EfUnitOfWork<AppDbContext>(
                serviceProvider.GetRequiredService<AppDbContext>(),
                serviceProvider.GetRequiredService<ILogger<EfUnitOfWork<AppDbContext>>>()
            )
        );

        services.AddScoped<IPageCache, PageCache>();

        return services;
    }
}
