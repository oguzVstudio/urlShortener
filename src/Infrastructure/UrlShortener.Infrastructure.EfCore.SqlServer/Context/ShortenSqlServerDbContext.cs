using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Context;

public class ShortenSqlServerDbContext : DbContext, IShortenBaseDbContext, ISupportSaveChanges
{
    public const string DefaultSchema = "shorten";

    public ShortenSqlServerDbContext(DbContextOptions<ShortenSqlServerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public DbSet<ShortenUrl> ShortenUrls => Set<ShortenUrl>();

    public DbSet<ShortenUrlTrack> ShortenUrlTracks => Set<ShortenUrlTrack>();
}