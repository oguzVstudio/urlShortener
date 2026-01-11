using Microsoft.Extensions.Caching.Distributed;
using UrlShortener.Application.Shared.Cache;

namespace UrlShortener.Infrastructure.Cache;

public class DistributedLock : IDistributedLock
{
    private readonly IDistributedCache _distributedCache;

    public DistributedLock(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }
    
    public async Task<bool> TryLockAsync(string key, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var  value = await _distributedCache.GetStringAsync(key, cancellationToken);
        var isLocked = !string.IsNullOrEmpty(value);
        if (isLocked) return false;
        
        await _distributedCache.SetStringAsync(key,"locked",new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeout
        },cancellationToken);

        return true;
    }

    public async Task<bool> TryRemoveAsync(string key, CancellationToken cancellationToken)
    {
        await  _distributedCache.RemoveAsync(key, cancellationToken);
        return true;
    }
}