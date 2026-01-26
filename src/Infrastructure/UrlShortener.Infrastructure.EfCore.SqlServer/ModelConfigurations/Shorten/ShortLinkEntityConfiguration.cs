using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.ModelConfigurations.Shorten;

public class ShortLinkEntityConfiguration : IEntityTypeConfiguration<ShortLink>
{
    public void Configure(EntityTypeBuilder<ShortLink> builder)
    {
        var prefix = !string.IsNullOrWhiteSpace(SqlServerDbSettings.TablePrefix)
            ? $"{SqlServerDbSettings.TablePrefix}"
            : string.Empty;
        
        var tableName = $"{prefix}ShortLinks";
        
        builder.ToTable(tableName, SqlServerDbSettings.DefaultSchema);
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
