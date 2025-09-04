using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pointr.CaseStudy.Application.Pages.ArchiveAndOptionallyPublish;

namespace Pointr.CaseStudy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCaseStudyApplication(this IServiceCollection services)
    {
        services.AddScoped<ArchiveAndOptionallyPublishHandler>();
        return services;
    }
}
