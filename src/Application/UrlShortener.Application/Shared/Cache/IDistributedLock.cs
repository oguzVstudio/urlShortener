namespace UrlShortener.Application.Shared.Cache;

public interface IDistributedLock
{
    Task<bool> TryLockAsync(string key, TimeSpan timeout, CancellationToken cancellationToken);
    Task<bool> TryRemoveAsync(string key, CancellationToken cancellationToken);
}