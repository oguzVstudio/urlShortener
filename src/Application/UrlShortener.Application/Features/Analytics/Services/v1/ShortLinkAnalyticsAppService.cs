using UrlShortener.Application.Features.Analytics.Services.v1.Models;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.Analytics;

namespace UrlShortener.Application.Features.Analytics.Services.v1;

public class ShortLinkAnalyticsAppService : IShortLinkAnalyticsAppService
{
    private readonly IShortLinkStore _shortLinkStore;
    private readonly IShortLinkAccessLogStore _shortLinkAccessLogStore;
    private readonly IShortenBaseDbContext _context;
    
    public ShortLinkAnalyticsAppService(
        IShortLinkStore shortLinkStore,
        IShortLinkAccessLogStore shortLinkAccessLogStore,
        IShortenBaseDbContext context)
    {
        _shortLinkStore = shortLinkStore;
        _shortLinkAccessLogStore = shortLinkAccessLogStore;
        _context = context;
    }
    
    public async Task<bool> CreateShortLinkAccessLogAsync(CreateShortLinkAccessLogRequest request, CancellationToken cancellationToken)
    {
        var shortLink = await _shortLinkStore.GetByCodeAsync(request.Code, cancellationToken);

        if (shortLink is null)
        {
            return false;
        }

        var accessLog = ShortLinkAccessLog.Create(shortLink.Id,
            request.Code,
            request.IpAddress,
            request.UserAgent,
            request.AccessedAt);
        
        await _shortLinkAccessLogStore.CreateAsync(accessLog, cancellationToken);
        await SaveAsync(cancellationToken);
        return true;
    }
    
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_context is ISupportSaveChanges supportSaveChanges)
            await supportSaveChanges.SaveChangesAsync(cancellationToken);
        return;
    }
}