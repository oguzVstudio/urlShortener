using MongoDB.Driver;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.ShortenUrls;
using UrlShortener.Infrastructure.MongoDB.Context;

namespace UrlShortener.Infrastructure.MongoDB.Stores;

public class ShortenUrlMongoStore(ShortenMongoDbContext context) : IShortenUrlStore
{
    private IMongoCollection<ShortenUrl> ShortenUrls
        => context.GetCollection<ShortenUrl>(nameof(ShortenUrl));

    public async Task CreateAsync(ShortenUrl shortenUrl, CancellationToken cancellationToken)
    {
        await ShortenUrls.InsertOneAsync(shortenUrl, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(ShortenUrl shortenUrl, CancellationToken cancellationToken)
    {
        await ShortenUrls.ReplaceOneAsync(x => x.Id == shortenUrl.Id,
            shortenUrl, cancellationToken: cancellationToken);
    }

    public async Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        return await ShortenUrls.Find(x => x.Code == code).AnyAsync(cancellationToken);
    }

    public async Task<ShortenUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await ShortenUrls.Find(x => x.Code == code).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ShortenUrl?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await ShortenUrls.Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetOriginalUrlAsync(string code, CancellationToken cancellationToken)
    {
        return await ShortenUrls
            .Find(x => x.Code == code
                       && (!x.IsExpiring || x.ExpiresAt > DateTimeOffset.UtcNow))
            .Project(x => x.LongUrl)
            .FirstOrDefaultAsync(cancellationToken);
    }
}