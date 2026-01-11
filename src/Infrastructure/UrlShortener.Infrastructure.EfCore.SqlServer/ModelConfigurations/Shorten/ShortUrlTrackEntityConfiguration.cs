using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Shorten.ShortenUrls;
using UrlShortener.Infrastructure.EfCore.SqlServer.Context;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.ModelConfigurations.Shorten;

public class ShortUrlTrackEntityConfiguration : IEntityTypeConfiguration<ShortenUrlTrack>
{
    public void Configure(EntityTypeBuilder<ShortenUrlTrack> builder)
    {
        builder.ToTable("ShortUrlTracks", ShortenSqlServerDbContext.DefaultSchema);
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
