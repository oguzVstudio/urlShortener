using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Domain.Shorten;

public interface IShortenUrlTrackStore
{
    Task CreateAsync(ShortenUrlTrack shortenUrlTrack, CancellationToken cancellationToken);
}