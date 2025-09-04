using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pointr.CaseStudy.Infrastructure.Persistence.Database;

public static class RowVersionBootstrapExtensions
{
    /// <summary>
    /// RowVersion for PostgreSQL (pgcrypto, sütun, tetikleyici).
    /// </summary>
    /// <param name="services"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task EnsureRowVersionInfrastructureAsync(
        this IServiceProvider services,
        CancellationToken ct = default
    )
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope
            .ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RowVersionBootstrap");

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // 1) pgcrypto
        await db.Database.ExecuteSqlRawAsync(@"CREATE EXTENSION IF NOT EXISTS pgcrypto;", ct);

        // 2) Pages.RowVersion: bytea + NOT NULL + DEFAULT
        await db.Database.ExecuteSqlRawAsync(
            @"
            ALTER TABLE ""Pages""
              ADD COLUMN IF NOT EXISTS ""RowVersion"" bytea NOT NULL DEFAULT gen_random_bytes(8);",
            ct
        );

        // 3) Trigger function: renew RowVersion at UPDATE
        await db.Database.ExecuteSqlRawAsync(
            @"
            CREATE OR REPLACE FUNCTION set_rowversion()
            RETURNS trigger AS $$
            BEGIN
              NEW.""RowVersion"" := gen_random_bytes(8);
              RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;",
            ct
        );

        // 4) Trigger (if not exists)
        await db.Database.ExecuteSqlRawAsync(
            @"
            DO $$
            BEGIN
              IF NOT EXISTS (
                SELECT 1
                FROM pg_trigger t
                JOIN pg_class c ON c.oid = t.tgrelid
                WHERE t.tgname = 'trg_set_rowversion' AND c.relname = 'Pages'
              ) THEN
                CREATE TRIGGER trg_set_rowversion
                BEFORE UPDATE ON ""Pages""
                FOR EACH ROW
                EXECUTE FUNCTION set_rowversion();
              END IF;
            END $$;",
            ct
        );

        await tx.CommitAsync(ct);
        logger.LogInformation("RowVersion infrastructure ensured (extension/column/trigger).");
    }
}
