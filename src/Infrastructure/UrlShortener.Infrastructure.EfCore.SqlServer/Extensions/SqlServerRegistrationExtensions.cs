using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten;
using UrlShortener.Infrastructure.EfCore.SqlServer.Context;
using UrlShortener.Infrastructure.EfCore.SqlServer.Stores;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Extensions;

public static class SqlServerRegistrationExtensions
{
    /// <summary>
    /// Register complete SQL Server persistence layer for URL Shortener domain
    /// </summary>
    public static IServiceCollection AddShortenSqlServer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddShortenSqlServerContext(configuration)
            .AddShortenSqlServerStores();
    }
    
    private static IServiceCollection AddShortenSqlServerContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SqlServerOptions>().BindConfiguration(nameof(SqlServerOptions));
        services.AddSingleton(x => x.GetRequiredService<IOptions<SqlServerOptions>>().Value);

        services.AddDbContext<ShortenSqlServerDbContext>((sp, options) =>
        {
            var sqlServerOptions = sp.GetRequiredService<SqlServerOptions>();
            
            ApplySqlServerDbSettings(sqlServerOptions);
            
            var migrationDefaultSchema = sqlServerOptions.MigrationDefaultSchema;
            if (string.IsNullOrWhiteSpace(migrationDefaultSchema))
                migrationDefaultSchema = "dbo";
            
            options.UseSqlServer(sqlServerOptions.ConnectionString,
                sqlOptions =>
                {
                    var name = sqlServerOptions.MigrationAssembly ??
                               typeof(ShortenSqlServerDbContext).Assembly.GetName().Name;

                    sqlOptions.MigrationsAssembly(name);
                    sqlOptions.MigrationsHistoryTable(
                        $"EfCore{migrationDefaultSchema}MigrationHistory", migrationDefaultSchema);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });
        });

        services.AddScoped<IShortenBaseDbContext>(provider => provider.GetRequiredService<ShortenSqlServerDbContext>());

        return services;
    }

    public static IServiceCollection AddShortenSqlServerStores(this IServiceCollection services)
    {
        services.AddScoped<IShortLinkStore, ShortLinkSqlServerStore>();
        services.AddScoped<IShortLinkAccessLogStore, ShortLinkAccessLogSqlServerStore>();

        return services;
    }
    
    internal static void ApplySqlServerDbSettings(SqlServerOptions sqlServerOptions)
    {
        SqlServerDbSettings.DefaultSchema = sqlServerOptions.DefaultSchema;
        SqlServerDbSettings.TablePrefix = sqlServerOptions.TablePrefix;
    }
}