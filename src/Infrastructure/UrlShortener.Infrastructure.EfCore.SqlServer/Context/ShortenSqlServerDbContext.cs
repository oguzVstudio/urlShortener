using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten.Analytics;
using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Context;

public class ShortenSqlServerDbContext : DbContext, IShortenBaseDbContext, ISupportSaveChanges
{
    public ShortenSqlServerDbContext(DbContextOptions<ShortenSqlServerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public DbSet<ShortLink> ShortLinks => Set<ShortLink>();

    public DbSet<ShortLinkAccessLog> ShortLinkAccessLogs => Set<ShortLinkAccessLog>();
}