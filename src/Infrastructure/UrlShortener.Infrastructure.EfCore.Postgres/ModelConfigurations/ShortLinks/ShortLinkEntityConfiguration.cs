using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Extensions;
using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Infrastructure.EfCore.Postgres.ModelConfigurations.Shorten;

public class ShortLinkEntityConfiguration : IEntityTypeConfiguration<ShortLink>
{
    public void Configure(EntityTypeBuilder<ShortLink> builder)
    {
        var prefix = !string.IsNullOrWhiteSpace(PostgresDbSettings.TablePrefix)
            ? $"{PostgresDbSettings.TablePrefix.ToSnakeCase()}_"
            : string.Empty;
        
        var tableName = $"{prefix}short_links";
        
        builder.ToTable(tableName, PostgresDbSettings.DefaultSchema);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder
            .Property(sl => sl.Code)
            .HasMaxLength(10);

        builder
            .HasIndex(sl => sl.Code)
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