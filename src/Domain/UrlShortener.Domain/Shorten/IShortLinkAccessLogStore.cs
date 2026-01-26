using UrlShortener.Domain.Shorten.Analytics;

namespace UrlShortener.Domain.Shorten;

public interface IShortLinkAccessLogStore
{
    Task CreateAsync(ShortLinkAccessLog accessLog, CancellationToken cancellationToken);
}