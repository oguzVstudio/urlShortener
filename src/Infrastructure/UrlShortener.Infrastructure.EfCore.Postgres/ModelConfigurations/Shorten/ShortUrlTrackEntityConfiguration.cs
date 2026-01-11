using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Extensions;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Infrastructure.EfCore.Postgres.ModelConfigurations.Shorten;

public class ShortUrlTrackEntityConfiguration : IEntityTypeConfiguration<ShortenUrlTrack>
{
    public void Configure(EntityTypeBuilder<ShortenUrlTrack> builder)
    {
        var prefix = !string.IsNullOrWhiteSpace(PostgresDbSettings.TablePrefix)
            ? $"{PostgresDbSettings.TablePrefix.ToSnakeCase()}_"
            : string.Empty;
        
        var tableName = $"{prefix}short_url_tracks";
        
        builder.ToTable(tableName, PostgresDbSettings.DefaultSchema);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.HasIndex(x => x.ShortenUrlId);

        builder
            .Property(shortenedUrl => shortenedUrl.Code)
            .HasMaxLength(10);

        builder
            .HasIndex(shortenedUrl => shortenedUrl.Code);

        builder
            .Property(shortenedUrl => shortenedUrl.UserAgent)
            .HasMaxLength(500);

        builder
            .Property(shortenedUrl => shortenedUrl.IpAddress)
            .HasMaxLength(20);

        builder.Property(x => x.AccessedAt)
            .HasColumnType("timestamptz")
            .HasConversion(
                v => v,
                v => v.ToUniversalTime()
            );

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("timestamptz")
            .HasConversion(
                v => v,
                v => v.ToUniversalTime()
            );
    }
}