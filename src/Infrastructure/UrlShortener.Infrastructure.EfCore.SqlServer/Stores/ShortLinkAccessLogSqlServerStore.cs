using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.Analytics;
using UrlShortener.Infrastructure.EfCore.SqlServer.Context;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Stores;

public class ShortLinkAccessLogSqlServerStore(ShortenSqlServerDbContext context) : IShortLinkAccessLogStore
{
    public async Task CreateAsync(ShortLinkAccessLog accessLog, CancellationToken cancellationToken)
    {
        await context.ShortLinkAccessLogs.AddAsync(accessLog, cancellationToken);
    }
}