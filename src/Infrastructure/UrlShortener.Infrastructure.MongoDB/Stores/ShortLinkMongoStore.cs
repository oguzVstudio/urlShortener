using MongoDB.Driver;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.ShortLinks;
using UrlShortener.Infrastructure.MongoDB.Context;

namespace UrlShortener.Infrastructure.MongoDB.Stores;

public class ShortLinkMongoStore(ShortenMongoDbContext context) : IShortLinkStore
{
    private IMongoCollection<ShortLink> ShortLinks
        => context.GetCollection<ShortLink>(nameof(ShortLink));

    public async Task CreateAsync(ShortLink shortLink, CancellationToken cancellationToken)
    {
        await ShortLinks.InsertOneAsync(shortLink, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(ShortLink shortLink, CancellationToken cancellationToken)
    {
        await ShortLinks.ReplaceOneAsync(x => x.Id == shortLink.Id,
            shortLink, cancellationToken: cancellationToken);
    }

    public async Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        return await ShortLinks.Find(x => x.Code == code).AnyAsync(cancellationToken);
    }

    public async Task<ShortLink?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await ShortLinks.Find(x => x.Code == code).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ShortLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await ShortLinks.Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetOriginalUrlAsync(string code, CancellationToken cancellationToken)
    {
        return await ShortLinks
            .Find(x => x.Code == code
                       && (!x.IsExpiring || x.ExpiresAt > DateTimeOffset.UtcNow))
            .Project(x => x.LongUrl)
            .FirstOrDefaultAsync(cancellationToken);
    }
}