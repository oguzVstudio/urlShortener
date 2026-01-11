using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Testcontainers.Redis;
using UrlShortener.Infrastructure.Cache;

namespace UrlShortener.Infrastructure.Tests.Cache;

public class DistributedLockTests : IAsyncLifetime
{
    private RedisContainer? _redisContainer;
    private IDistributedCache? _cache;
    private DistributedLock? _distributedLock;

    public async Task InitializeAsync()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();

        await _redisContainer.StartAsync();

        var options = Options.Create(new RedisCacheOptions
        {
            Configuration = _redisContainer.GetConnectionString()
        });

        _cache = new RedisCache(options);
        _distributedLock = new DistributedLock(_cache);
    }

    public async Task DisposeAsync()
    {
        if (_redisContainer != null)
        {
            await _redisContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task TryLockAsync_ShouldAcquireLock_WhenLockIsAvailable()
    {
        // Arrange
        var key = $"test-lock-{Guid.NewGuid()}";
        var timeout = TimeSpan.FromSeconds(10);

        // Act
        var result = await _distributedLock!.TryLockAsync(key, timeout, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryLockAsync_ShouldFailToAcquireLock_WhenLockIsAlreadyHeld()
    {
        // Arrange
        var key = $"test-lock-{Guid.NewGuid()}";
        var timeout = TimeSpan.FromSeconds(10);

        // Act
        var firstLock = await _distributedLock!.TryLockAsync(key, timeout, CancellationToken.None);
        var secondLock = await _distributedLock.TryLockAsync(key, timeout, CancellationToken.None);

        // Assert
        firstLock.Should().BeTrue();
        secondLock.Should().BeFalse();
    }

    [Fact]
    public async Task TryLockAsync_ShouldAcquireLock_AfterLockExpires()
    {
        // Arrange
        var key = $"test-lock-{Guid.NewGuid()}";
        var shortTimeout = TimeSpan.FromSeconds(1);

        // Act
        var firstLock = await _distributedLock!.TryLockAsync(key, shortTimeout, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Wait for lock to expire
        var secondLock = await _distributedLock.TryLockAsync(key, shortTimeout, CancellationToken.None);

        // Assert
        firstLock.Should().BeTrue();
        secondLock.Should().BeTrue();
    }

    [Fact]
    public async Task TryRemoveAsync_ShouldRemoveLock_WhenLockExists()
    {
        // Arrange
        var key = $"test-lock-{Guid.NewGuid()}";
        var timeout = TimeSpan.FromSeconds(10);

        await _distributedLock!.TryLockAsync(key, timeout, CancellationToken.None);

        // Act
        var removeResult = await _distributedLock.TryRemoveAsync(key, CancellationToken.None);
        var lockAfterRemove = await _distributedLock.TryLockAsync(key, timeout, CancellationToken.None);

        // Assert
        removeResult.Should().BeTrue();
        lockAfterRemove.Should().BeTrue(); // Can acquire lock after removal
    }

    [Fact]
    public async Task TryRemoveAsync_ShouldReturnTrue_WhenLockDoesNotExist()
    {
        // Arrange
        var key = $"test-lock-{Guid.NewGuid()}";

        // Act
        var result = await _distributedLock!.TryRemoveAsync(key, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task MultipleLocks_ShouldBeIndependent()
    {
        // Arrange
        var key1 = $"test-lock-1-{Guid.NewGuid()}";
        var key2 = $"test-lock-2-{Guid.NewGuid()}";
        var timeout = TimeSpan.FromSeconds(10);

        // Act
        var lock1 = await _distributedLock!.TryLockAsync(key1, timeout, CancellationToken.None);
        var lock2 = await _distributedLock.TryLockAsync(key2, timeout, CancellationToken.None);

        // Assert
        lock1.Should().BeTrue();
        lock2.Should().BeTrue();
    }
}
