using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Shorten.Analytics;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.ModelConfigurations.Analytics;

public class ShortLinkAccessLogEntityConfiguration : IEntityTypeConfiguration<ShortLinkAccessLog>
{
    public void Configure(EntityTypeBuilder<ShortLinkAccessLog> builder)
    {
         var prefix = !string.IsNullOrWhiteSpace(SqlServerDbSettings.TablePrefix)
            ? $"{SqlServerDbSettings.TablePrefix}"
            : string.Empty;
        
        var tableName = $"{prefix}ShortLinkAccessLogs";
        
        builder.ToTable(tableName, SqlServerDbSettings.DefaultSchema);
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
          .HasConversion(
              v => v,
              v => v.ToUniversalTime()
          );

        builder.Property(x => x.CreatedOnUtc)
          .HasConversion(
              v => v,
              v => v.ToUniversalTime()
          );
    }
}
