using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Domain.Shorten;

public interface IShortenUrlStore
{
    Task CreateAsync(ShortenUrl shortenUrl, CancellationToken cancellationToken);
    Task UpdateAsync(ShortenUrl shortenUrl, CancellationToken cancellationToken);
    Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken);
    Task<ShortenUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<ShortenUrl?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<string?> GetOriginalUrlAsync(string code, CancellationToken cancellationToken);
}