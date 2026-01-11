using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.ShortenUrls;
using UrlShortener.Infrastructure.EfCore.SqlServer.Context;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Stores;

public class ShortenUrlSqlServerStore(ShortenSqlServerDbContext context) : IShortenUrlStore
{
    public async Task CreateAsync(ShortenUrl shortenUrl, CancellationToken cancellationToken)
    {
        await context.ShortenUrls.AddAsync(shortenUrl, cancellationToken);
    }

    public Task UpdateAsync(ShortenUrl shortenUrl, CancellationToken cancellationToken)
    {
        context.ShortenUrls.Update(shortenUrl);
        return Task.CompletedTask;
    }

    public async Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        return await context.ShortenUrls.AnyAsync(x => x.Code == code, cancellationToken);
    }

    public async Task<ShortenUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await context.ShortenUrls
            .Where(x => x.Code == code)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ShortenUrl?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.ShortenUrls
            .FindAsync([id], cancellationToken);
    }

    public async Task<string?> GetOriginalUrlAsync(string code, CancellationToken cancellationToken)
    {
        return await context.ShortenUrls
            .Where(x => x.Code == code
                        && (!x.IsExpiring || x.ExpiresAt > DateTimeOffset.UtcNow))
            .Select(x => x.LongUrl).FirstOrDefaultAsync(cancellationToken);
    }
}