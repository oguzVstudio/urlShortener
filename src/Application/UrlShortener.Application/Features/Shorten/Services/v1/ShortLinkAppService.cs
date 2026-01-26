using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using UrlShortener.Application.Features.Shared.Cache;
using UrlShortener.Application.Features.Shorten.Services.v1.Models;
using UrlShortener.Application.Services.Caching;
using UrlShortener.Application.Services.CodeGenerators;
using UrlShortener.Application.Shared.Settings;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Application.Features.Shorten.Services.v1;

public class ShortLinkAppService : IShortLinkAppService
{
    private readonly IShortLinkStore _shortLinkStore;
    private readonly IShortenBaseDbContext _context;
    private readonly HybridCache _hybridCache;
    private readonly IDistributedLock _distributedLock;
    private readonly IUniqueCodeGenerator _uniqueCodeGenerator;
    private readonly ShortLinkSettings _shortLinkSettings;

    private const string ShortLinkCacheKeyPrefix = "shortLink:";
    private const string ShortLinkCodeLockKeyPrefix = "shortLinkCodeLock:";

    public ShortLinkAppService(
        IShortLinkStore shortLinkStore,
        IShortenBaseDbContext context,
        HybridCache hybridCache,
        IDistributedLock distributedLock,
        IUniqueCodeGenerator uniqueCodeGenerator,
        IOptions<ShortLinkSettings> shortLinkSettings)
    {
        _shortLinkStore = shortLinkStore;
        _context = context;
        _hybridCache = hybridCache;
        _distributedLock = distributedLock;
        _uniqueCodeGenerator = uniqueCodeGenerator;
        _shortLinkSettings = shortLinkSettings.Value;
    }

    public async Task<CreateShortLinkResponse> CreateShortLinkAsync(CreateShortLinkRequest request,
        CancellationToken cancellationToken)
    {
        var code = await GenerateCodeAsync(cancellationToken);
        var shortUrl = $"{_shortLinkSettings.BaseUrl}/{code}";

        var shortLink = ShortLink.Create(request.Url,
            shortUrl,
            code,
            request.IsExpiring,
            request.ExpiresAt);

        await _shortLinkStore.CreateAsync(shortLink, cancellationToken);
        await SaveAsync(cancellationToken);
        await _distributedLock.TryRemoveAsync(
            $"{ShortLinkCodeLockKeyPrefix}{code}",
            cancellationToken);
        return new CreateShortLinkResponse(shortUrl, code, true);
    }

    public async Task<GetOriginalUrlResponse> GetOriginalUrlAsync(string code, CancellationToken cancellationToken)
    {
        var cacheKey = new ShortenDistributedCacheKey($"{ShortLinkCacheKeyPrefix}{code}");

        var originalUrl = await _hybridCache.GetOrCreateAsync<string>(
            cacheKey.Key,
            async entry =>
            {
                var url = await _shortLinkStore.GetOriginalUrlAsync(code, entry);
                return url ?? string.Empty;
            }, cancellationToken: cancellationToken);

        return new GetOriginalUrlResponse(originalUrl,
            !string.IsNullOrWhiteSpace(originalUrl));
    }

    private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
    {
        string code;
        bool generated;
        do
        {
            code = await _uniqueCodeGenerator.GenerateAsync(cancellationToken);
            var isLocked = await _distributedLock.TryLockAsync(
                $"{ShortLinkCodeLockKeyPrefix}{code}",
                TimeSpan.FromMinutes(10),
                cancellationToken);

            if (isLocked)
            {
                var exists = await _shortLinkStore.IsCodeExistsAsync(code, cancellationToken);
                generated = !exists;
            }
            else
            {
                generated = false;
            }
        } while (!generated);

        return code;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_context is ISupportSaveChanges supportSaveChanges)
            await supportSaveChanges.SaveChangesAsync(cancellationToken);
        return;
    }
}