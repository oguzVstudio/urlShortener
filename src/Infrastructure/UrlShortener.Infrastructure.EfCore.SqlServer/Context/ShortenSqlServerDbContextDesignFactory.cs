using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using UrlShortener.Infrastructure.EfCore.SqlServer.Extensions;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Context;

public class ShortenSqlServerDbContextDesignFactory : IDesignTimeDbContextFactory<ShortenSqlServerDbContext>
{
    private readonly string _migrationDefaultSchema = "dbo";
    private readonly string _connectionStringSection = "SqlServerOptions:ConnectionString";
    private readonly string _section = "SqlServerOptions";

    public ShortenSqlServerDbContext CreateDbContext(string[] args)
    {
        var builder = GetConfigurationBuilder(args);
        var configuration = builder.Build();
        
        var sqlServerOptions = GetPostgresOptions(configuration);
        var connectionString = sqlServerOptions.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Could not find a value for {_connectionStringSection} section.");
        }

        var migrationDefaultSchema = sqlServerOptions.MigrationDefaultSchema;
        if (string.IsNullOrWhiteSpace(migrationDefaultSchema))
            migrationDefaultSchema = _migrationDefaultSchema;

        SqlServerRegistrationExtensions.ApplySqlServerDbSettings(sqlServerOptions);

        var optionsBuilder = new DbContextOptionsBuilder<ShortenSqlServerDbContext>()
            .UseSqlServer(connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(GetType().Assembly.FullName);
                    
                    sqlOptions.MigrationsHistoryTable(
                        $"EfCore{migrationDefaultSchema}MigrationHistory", migrationDefaultSchema);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(20).TotalSeconds);
                });

        return (ShortenSqlServerDbContext)Activator.CreateInstance(typeof(ShortenSqlServerDbContext), optionsBuilder.Options)!;
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
    
    private SqlServerOptions GetPostgresOptions(IConfigurationRoot configuration)
    {
        var section = configuration.GetSection(_section);
        var sqlServerOptions = section.Get<SqlServerOptions>();
        configuration.Bind(sqlServerOptions);
        return sqlServerOptions!;
    }
}
