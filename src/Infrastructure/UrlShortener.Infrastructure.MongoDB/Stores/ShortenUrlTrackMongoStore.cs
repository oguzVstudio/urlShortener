using MongoDB.Driver;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.ShortenUrls;
using UrlShortener.Infrastructure.MongoDB.Context;

namespace UrlShortener.Infrastructure.MongoDB.Stores;

public class ShortenUrlTrackMongoStore(ShortenMongoDbContext context) : IShortenUrlTrackStore
{
    private IMongoCollection<ShortenUrlTrack> ShortenUrlTracks
        => context.GetCollection<ShortenUrlTrack>(nameof(ShortenUrlTrack));

    public async Task CreateAsync(ShortenUrlTrack shortenUrlTrack, CancellationToken cancellationToken)
    {
        await ShortenUrlTracks.InsertOneAsync(shortenUrlTrack, cancellationToken: cancellationToken);
    }
}