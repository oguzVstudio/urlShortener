using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using UrlShortener.Application.Features.Shared.Cache;
using UrlShortener.Application.Features.Shorten.Services.v1.Models;
using UrlShortener.Application.Services.CodeGenerators;
using UrlShortener.Application.Shared.Cache;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.Settings;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Application.Features.Shorten.Services.v1;

public class ShortenUrlAppService : IShortenUrlAppService
{
    private readonly IShortenUrlStore _shortenUrlStore;
    private readonly IShortenUrlTrackStore _shortenUrlTrackStore;
    private readonly IShortenBaseDbContext _context;
    private readonly HybridCache _hybridCache;
    private readonly IDistributedLock _distributedLock;
    private readonly IUniqueCodeGenerator _uniqueCodeGenerator;
    private readonly ShortenUrlSettings _shortenUrlSettings;

    private const string ShortUrlCacheKeyPrefix = "shortUrl:";
    private const string ShortUrlCodeLockKeyPrefix = "shortUrlCodeLock:";

    public ShortenUrlAppService(
        IShortenUrlStore shortenUrlStore,
        IShortenUrlTrackStore shortenUrlTrackStore,
        IShortenBaseDbContext context,
        HybridCache hybridCache,
        IDistributedLock distributedLock,
        IUniqueCodeGenerator uniqueCodeGenerator,
        IOptions<ShortenUrlSettings> shortenUrlSettings)
    {
        _shortenUrlStore = shortenUrlStore;
        _shortenUrlTrackStore = shortenUrlTrackStore;
        _context = context;
        _hybridCache = hybridCache;
        _distributedLock = distributedLock;
        _uniqueCodeGenerator = uniqueCodeGenerator;
        _shortenUrlSettings = shortenUrlSettings.Value;
    }

    public async Task<CreateShortUrlResponse> ShortenUrlAsync(CreateShortUrlRequest request,
        CancellationToken cancellationToken)
    {
        var code = await GenerateCodeAsync(cancellationToken);
        var shortUrl = $"{_shortenUrlSettings.BaseUrl}/{code}";

        var shortenUrl = ShortenUrl.Create(request.Url,
            shortUrl,
            code,
            request.IsExpiring,
            request.ExpiresAt);

        await _shortenUrlStore.CreateAsync(shortenUrl, cancellationToken);
        await SaveAsync(cancellationToken);
        await _distributedLock.TryRemoveAsync(
            $"{ShortUrlCodeLockKeyPrefix}{code}",
            cancellationToken);
        return new CreateShortUrlResponse(shortUrl, code, true);
    }

    public async Task<GetOriginalUrlResponse> GetOriginalUrlAsync(string code, CancellationToken cancellationToken)
    {
        var cacheKey = new ShortenDistributedCacheKey($"{ShortUrlCacheKeyPrefix}{code}");

        var originalUrl = await _hybridCache.GetOrCreateAsync<string>(
            cacheKey.Key,
            async entry =>
            {
                var shortenedUrl = await _shortenUrlStore.GetOriginalUrlAsync(code, entry);
                return shortenedUrl ?? string.Empty;
            }, cancellationToken: cancellationToken);

        return new GetOriginalUrlResponse(originalUrl,
            !string.IsNullOrWhiteSpace(originalUrl));
    }

    public async Task<bool> TrackUrlAccessAsync(CreateShortUrlTrackRequest request, CancellationToken cancellationToken)
    {
        var shortenUrl = await _shortenUrlStore.GetByCodeAsync(request.Code, cancellationToken);

        if (shortenUrl is null)
        {
            return false;
        }

        shortenUrl.IncrementAttemptCount();

        var shortenUrlTrack = ShortenUrlTrack.Create(shortenUrl.Id,
            shortenUrl.Code,
            request.IpAddress,
            request.UserAgent,
            request.AccessedAt);

        await _shortenUrlTrackStore.CreateAsync(shortenUrlTrack, cancellationToken);
        await SaveAsync(cancellationToken);
        return true;
    }

    private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
    {
        string code;
        bool generated;
        do
        {
            code = await _uniqueCodeGenerator.GenerateAsync(cancellationToken);
            var isLocked = await _distributedLock.TryLockAsync(
                $"{ShortUrlCodeLockKeyPrefix}{code}",
                TimeSpan.FromMinutes(10),
                cancellationToken);

            if (isLocked)
            {
                var exists = await _shortenUrlStore.IsCodeExistsAsync(code, cancellationToken);
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