using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.ShortLinks;
using UrlShortener.Infrastructure.EfCore.SqlServer.Context;

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Stores;

public class ShortLinkSqlServerStore(ShortenSqlServerDbContext context) : IShortLinkStore
{
    public async Task CreateAsync(ShortLink shortLink, CancellationToken cancellationToken)
    {
        await context.ShortLinks.AddAsync(shortLink, cancellationToken);
    }

    public Task UpdateAsync(ShortLink shortLink, CancellationToken cancellationToken)
    {
        context.ShortLinks.Update(shortLink);
        return Task.CompletedTask;
    }

    public async Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        return await context.ShortLinks.AnyAsync(x => x.Code == code, cancellationToken);
    }

    public async Task<ShortLink?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await context.ShortLinks
            .Where(x => x.Code == code)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ShortLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.ShortLinks
            .FindAsync([id], cancellationToken);
    }

    public async Task<string?> GetOriginalUrlAsync(string code, CancellationToken cancellationToken)
    {
        return await context.ShortLinks
            .Where(x => x.Code == code
                        && (!x.IsExpiring || x.ExpiresAt > DateTimeOffset.UtcNow))
            .Select(x => x.LongUrl).FirstOrDefaultAsync(cancellationToken);
    }
}