using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.ShortenUrls;
using UrlShortener.Infrastructure.EfCore.SqlServer.Context;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Stores;

public class ShortenUrlTrackSqlServerStore(ShortenSqlServerDbContext context) : IShortenUrlTrackStore
{
    public async Task CreateAsync(ShortenUrlTrack shortenUrlTrack, CancellationToken cancellationToken)
    {
        await context.ShortenUrlTracks.AddAsync(shortenUrlTrack, cancellationToken);
    }
}