using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using UrlShortener.Infrastructure.EfCore.Postgres.Extensions;

namespace UrlShortener.Infrastructure.EfCore.Postgres.Context;

public class ShortenPostgresDbContextDesignFactory : IDesignTimeDbContextFactory<ShortenPostgresDbContext>
{
    private readonly string _migrationDefaultSchema = "public";
    private readonly string _connectionStringSection = "PostgresOptions:ConnectionString";
    private readonly string _section = "PostgresOptions";

    public ShortenPostgresDbContext CreateDbContext(string[] args)
    {
        var builder = GetConfigurationBuilder(args);
        var configuration = builder.Build();
        
        var postgresOptions = GetPostgresOptions(configuration);
        var connectionString = postgresOptions.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Could not find a value for {_connectionStringSection} section.");
        }

        var migrationDefaultSchema = postgresOptions.MigrationDefaultSchema;
        if (string.IsNullOrWhiteSpace(migrationDefaultSchema))
            migrationDefaultSchema = _migrationDefaultSchema;

        PostgresRegistrationExtensions.ApplyPostgresDbSettings(postgresOptions);
        
        var optionsBuilder = NpgsqlDbContextOptionsBuilderExtensions
            .UseNpgsql((DbContextOptionsBuilder)new DbContextOptionsBuilder<ShortenPostgresDbContext>(),
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(GetType().Assembly.FullName);
                    sqlOptions.MigrationsHistoryTable(
                        $"efcore_{migrationDefaultSchema}_migration_history", migrationDefaultSchema);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    sqlOptions.CommandTimeout(180);
                }
            );

        return (ShortenPostgresDbContext)Activator.CreateInstance(typeof(ShortenPostgresDbContext),
            optionsBuilder.Options)!;
    }

    public virtual IConfigurationBuilder GetConfigurationBuilder(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory ?? "")
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true)
            .AddEnvironmentVariables();

        Console.WriteLine(environmentName);
        return builder;
    }

    private PostgresOptions GetPostgresOptions(IConfigurationRoot configuration)
    {
        var section = configuration.GetSection(_section);
        var postgresOptions = section.Get<PostgresOptions>();
        configuration.Bind(postgresOptions);
        return postgresOptions!;
    }
}