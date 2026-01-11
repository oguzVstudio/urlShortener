namespace UrlShortener.Infrastructure.EfCore.Postgres;

public class PostgresOptions
{
    public string ConnectionString { get; set; } = default!;
    public bool UseInMemory { get; set; }
    public string InMemoryDbName { get; set; } = default!;
    public string MigrationAssembly { get; set; } = null!;
    public string MigrationDefaultSchema { get; set; } = "public";
    public string TablePrefix { get; set; } = "";
    public string DefaultSchema { get; set; } = "shorten";
}
