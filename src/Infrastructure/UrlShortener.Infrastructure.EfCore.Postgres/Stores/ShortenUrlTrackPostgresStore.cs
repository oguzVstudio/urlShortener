using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.ShortenUrls;
using UrlShortener.Infrastructure.EfCore.Postgres.Context;

namespace UrlShortener.Infrastructure.EfCore.Postgres.Stores;

public class ShortenUrlTrackPostgresStore(ShortenPostgresDbContext context) : IShortenUrlTrackStore
{
    public async Task CreateAsync(ShortenUrlTrack shortenUrlTrack, CancellationToken cancellationToken)
    {
        await context.ShortenUrlTracks.AddAsync(shortenUrlTrack, cancellationToken);
    }
}