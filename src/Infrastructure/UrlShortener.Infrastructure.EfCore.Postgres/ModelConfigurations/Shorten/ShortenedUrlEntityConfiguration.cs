using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Extensions;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Infrastructure.EfCore.Postgres.ModelConfigurations.Shorten;

public class ShortenedUrlEntityConfiguration : IEntityTypeConfiguration<ShortenUrl>
{
    public void Configure(EntityTypeBuilder<ShortenUrl> builder)
    {
        var prefix = !string.IsNullOrWhiteSpace(PostgresDbSettings.TablePrefix)
            ? $"{PostgresDbSettings.TablePrefix.ToSnakeCase()}_"
            : string.Empty;
        
        var tableName = $"{prefix}shorten_urls";
        
        builder.ToTable(tableName, PostgresDbSettings.DefaultSchema);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder
            .Property(shortenedUrl => shortenedUrl.Code)
            .HasMaxLength(10);

        builder
            .HasIndex(shortenedUrl => shortenedUrl.Code)
            .IsUnique();

        builder.Property(x => x.LongUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.ShortUrl)
            .HasMaxLength(512);

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("timestamptz")
            .HasConversion(
                v => v,
                v => v.ToUniversalTime()
            );

        builder.Property(x => x.ExpiresAt)
            .HasColumnType("timestamptz")
            .HasConversion(
                v => v,
                v => v.HasValue ? v.Value.ToUniversalTime() : null
            );
    }
}