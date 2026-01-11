using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UrlShortener.Infrastructure.EfCore.Postgres.Context;

namespace UrlShortener.Infrastructure.EfCore.Postgres.Extensions;

public static class PostgresMigrationExtensions
{
    public static async Task ApplyPostgresDatabaseMigrationsAsync(this IServiceProvider sp)
    {
        var configuration = sp.GetRequiredService<IConfiguration>();

        if (configuration.GetValue<bool>("PostgresOptions:UseInMemory") == false)
        {
            using var serviceScope = sp.CreateScope();
            var locationDbContext = serviceScope.ServiceProvider.GetRequiredService<ShortenPostgresDbContext>();

            var logger = sp.GetService<ILogger>();
            
            logger?.LogInformation("Updating database...");

            await locationDbContext.Database.MigrateAsync();

            logger?.LogInformation("Updated database");
        }
    }
}