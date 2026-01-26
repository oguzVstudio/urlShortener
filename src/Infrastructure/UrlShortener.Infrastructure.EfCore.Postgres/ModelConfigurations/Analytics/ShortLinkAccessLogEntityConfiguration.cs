using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Extensions;
using UrlShortener.Domain.Shorten.Analytics;

namespace UrlShortener.Infrastructure.EfCore.Postgres.ModelConfigurations.Analytics;

public class ShortLinkAccessLogEntityConfiguration : IEntityTypeConfiguration<ShortLinkAccessLog>
{
    public void Configure(EntityTypeBuilder<ShortLinkAccessLog> builder)
    {
        var prefix = !string.IsNullOrWhiteSpace(PostgresDbSettings.TablePrefix)
            ? $"{PostgresDbSettings.TablePrefix.ToSnakeCase()}_"
            : string.Empty;
        
        var tableName = $"{prefix}short_link_access_logs";
        
        builder.ToTable(tableName, PostgresDbSettings.DefaultSchema);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.HasIndex(x => x.ShortLinkId);

        builder
            .Property(log => log.Code)
            .HasMaxLength(10);

        builder
            .HasIndex(log => log.Code);

        builder
            .Property(log => log.UserAgent)
            .HasMaxLength(500);

        builder
            .Property(log => log.IpAddress)
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