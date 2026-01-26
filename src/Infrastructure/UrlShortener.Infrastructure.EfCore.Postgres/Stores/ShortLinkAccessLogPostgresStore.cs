using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.Analytics;
using UrlShortener.Infrastructure.EfCore.Postgres.Context;

namespace UrlShortener.Infrastructure.EfCore.Postgres.Stores;

public class ShortLinkAccessLogPostgresStore(ShortenPostgresDbContext context) : IShortLinkAccessLogStore
{
    public async Task CreateAsync(ShortLinkAccessLog accessLog, CancellationToken cancellationToken)
    {
        await context.ShortLinkAccessLogs.AddAsync(accessLog, cancellationToken);
    }
}