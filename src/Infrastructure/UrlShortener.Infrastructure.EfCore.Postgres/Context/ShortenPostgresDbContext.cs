using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten.Analytics;
using UrlShortener.Domain.Shorten.ShortLinks;
using UrlShortener.Infrastructure.EfCore.Postgres.Extensions;

namespace UrlShortener.Infrastructure.EfCore.Postgres.Context;

public class ShortenPostgresDbContext : DbContext, IShortenBaseDbContext, ISupportSaveChanges
{
    public ShortenPostgresDbContext(DbContextOptions<ShortenPostgresDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.ApplySnakeCaseNamingConvention();
    }

    public DbSet<ShortLink> ShortLinks => Set<ShortLink>();

    public DbSet<ShortLinkAccessLog> ShortLinkAccessLogs => Set<ShortLinkAccessLog>();
}