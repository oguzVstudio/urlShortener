using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Domain.Shorten;

public interface IShortLinkStore
{
    Task CreateAsync(ShortLink shortLink, CancellationToken cancellationToken);
    Task UpdateAsync(ShortLink shortLink, CancellationToken cancellationToken);
    Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken);
    Task<ShortLink?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<ShortLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<string?> GetOriginalUrlAsync(string code, CancellationToken cancellationToken);
}