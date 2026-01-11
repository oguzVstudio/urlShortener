using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten;
using UrlShortener.Infrastructure.EfCore.Postgres.Context;
using UrlShortener.Infrastructure.EfCore.Postgres.Stores;

namespace UrlShortener.Infrastructure.EfCore.Postgres.Extensions;

public static class PostgresRegistrationExtensions
{
    /// <summary>
    /// Register complete Postgres persistence layer (Context + Stores)
    /// </summary>
    public static IServiceCollection AddShortenPostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddShortenPostgresContext(configuration)
            .AddShortenPostgresStores();
    }

    /// <summary>
    /// Register Postgres DbContext with IConfiguration binding
    /// </summary>
    public static IServiceCollection AddShortenPostgresContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddOptions<PostgresOptions>().BindConfiguration(nameof(PostgresOptions));
        services.AddSingleton(x => x.GetRequiredService<IOptions<PostgresOptions>>().Value);

        services.AddDbContext<ShortenPostgresDbContext>((sp, options) =>
        {
            var postgresOptions = sp.GetRequiredService<PostgresOptions>();

            ApplyPostgresDbSettings(postgresOptions);
            
            var migrationDefaultSchema = postgresOptions.MigrationDefaultSchema;
            if (string.IsNullOrWhiteSpace(migrationDefaultSchema))
                migrationDefaultSchema = "public";

            options.UseNpgsql(postgresOptions.ConnectionString,
                sqlOptions =>
                {
                    var name = postgresOptions.MigrationAssembly ??
                               typeof(ShortenPostgresDbContext).Assembly.GetName().Name;

                    sqlOptions.MigrationsAssembly(name);
                    sqlOptions.MigrationsHistoryTable(
                        $"efcore_{migrationDefaultSchema}_migration_history", migrationDefaultSchema);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });
        });

        services.AddScoped<IShortenBaseDbContext>(provider => provider.GetRequiredService<ShortenPostgresDbContext>());

        return services;
    }

    public static IServiceCollection AddShortenPostgresStores(this IServiceCollection services)
    {
        services.AddScoped<IShortenUrlStore, ShortenUrlPostgresStore>();
        services.AddScoped<IShortenUrlTrackStore, ShortenUrlTrackPostgresStore>();

        return services;
    }

    internal static void ApplyPostgresDbSettings(PostgresOptions postgresOptions)
    {
        PostgresDbSettings.DefaultSchema = postgresOptions.DefaultSchema;
        PostgresDbSettings.TablePrefix = postgresOptions.TablePrefix;
    }
}