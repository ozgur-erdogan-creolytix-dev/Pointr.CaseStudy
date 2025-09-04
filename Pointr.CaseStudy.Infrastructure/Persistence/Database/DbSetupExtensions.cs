using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pointr.CaseStudy.Infrastructure.Persistence.Database;

public static class DbSetupExtensions
{
    public static async Task EnsureCaseStudyDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        // Create a scoped service provider for resolving scoped services
        using var scope = serviceProvider.CreateScope();

        // Resolve DbContext and logger
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope
            .ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseSetup");

        // Check if there are pending EF Core migrations
        var hasMigrations = dbContext.Database.GetMigrations().Any();

        if (hasMigrations)
        {
            logger.LogInformation("Applying EF Core migrations...");
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migrations applied successfully.");
        }
        else
        {
            logger.LogWarning(
                "No migrations found. Running EnsureCreated() to build schema from model."
            );
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation("Database schema created using EnsureCreated.");
        }
    }
}
