using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.ModelConfigurations.Shorten;

public class ShortenedUrlEntityConfiguration : IEntityTypeConfiguration<ShortenUrl>
{
    public void Configure(EntityTypeBuilder<ShortenUrl> builder)
    {
        var prefix = !string.IsNullOrWhiteSpace(SqlServerDbSettings.TablePrefix)
            ? $"{SqlServerDbSettings.TablePrefix}"
            : string.Empty;
        
        var tableName = $"{prefix}ShortenUrls";
        
        builder.ToTable(tableName, SqlServerDbSettings.DefaultSchema);
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
          .HasConversion(
              v => v,
              v => v.ToUniversalTime()
          );

        builder.Property(x => x.ExpiresAt)
          .HasConversion(
              v => v,
              v => v.HasValue ? v.Value.ToUniversalTime() : null
          );
    }
}
