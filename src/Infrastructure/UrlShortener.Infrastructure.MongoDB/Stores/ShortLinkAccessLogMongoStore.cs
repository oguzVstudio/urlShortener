using MongoDB.Driver;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.Analytics;
using UrlShortener.Infrastructure.MongoDB.Context;

namespace UrlShortener.Infrastructure.MongoDB.Stores;

public class ShortLinkAccessLogMongoStore(ShortenMongoDbContext context) : IShortLinkAccessLogStore
{
    private IMongoCollection<ShortLinkAccessLog> ShortLinkAccessLogs
        => context.GetCollection<ShortLinkAccessLog>(nameof(ShortLinkAccessLog));

    public async Task CreateAsync(ShortLinkAccessLog accessLog, CancellationToken cancellationToken)
    {
        await ShortLinkAccessLogs.InsertOneAsync(accessLog, cancellationToken: cancellationToken);
    }
}