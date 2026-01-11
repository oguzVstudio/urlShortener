using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UrlShortener.Infrastructure.EfCore.SqlServer.Context;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Extensions;

public static class SqlServerMigrationExtensions
{
    public static async Task ApplySqlServerDatabaseMigrationsAsync(this IServiceProvider sp)
    {
        var configuration = sp.GetRequiredService<IConfiguration>();

        if (!configuration.GetValue<bool>("SqlServerOptions:UseInMemory"))
        {
            using var serviceScope = sp.CreateScope();
            var locationDbContext = serviceScope.ServiceProvider.GetRequiredService<ShortenSqlServerDbContext>();

            var logger = sp.GetService<ILogger>();
            
            logger?.LogInformation("Updating database...");

            await locationDbContext.Database.MigrateAsync();

            logger?.LogInformation("Updated database");
        }
    }
}