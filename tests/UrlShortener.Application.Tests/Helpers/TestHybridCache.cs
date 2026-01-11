using Microsoft.Extensions.Caching.Hybrid;

namespace UrlShortener.Application.Tests.Helpers;

/// <summary>
/// Test implementation of HybridCache for testing purposes
/// </summary>
public class TestHybridCache : HybridCache
{
    private readonly Dictionary<string, object?> _cache = new();

    public void Set<T>(string key, T value)
    {
        _cache[key] = value;
    }

    public override async ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is T typedValue)
        {
            return typedValue;
        }

        var value = await factory(state, cancellationToken);
        _cache[key] = value;
        return value;
    }

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        // Simple implementation - in real scenarios, tags would be tracked
        return ValueTask.CompletedTask;
    }

    public override ValueTask SetAsync<T>(string key, T value, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        _cache[key] = value;
        return ValueTask.CompletedTask;
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
