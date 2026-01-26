using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Shorten.ShortLinks;
using UrlShortener.Infrastructure.EfCore.Postgres.Context;
using UrlShortener.Infrastructure.EfCore.Postgres.Stores;

namespace UrlShortener.Infrastructure.Tests.Stores;

public class ShortLinkStoreTests : IAsyncLifetime
{
    private ShortenPostgresDbContext _dbContext = null!;
    private ShortLinkPostgresStore _store = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ShortenPostgresDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ShortenPostgresDbContext(options);
        _store = new ShortLinkPostgresStore(_dbContext);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddShortenUrlToDatabase()
    {
        // Arrange
        var shortenUrl = ShortLink.Create(
            "https://www.example.com/long/url",
            "https://short.url/abc123",
            "abc123");
        var cancellationToken = CancellationToken.None;

        // Act
        await _store.CreateAsync(shortenUrl, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Assert
        var result = await _dbContext.ShortLinks.FirstOrDefaultAsync(x => x.Code == "abc123", cancellationToken);
        result.Should().NotBeNull();
        result!.LongUrl.Should().Be("https://www.example.com/long/url");
        result.Code.Should().Be("abc123");
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnShortenUrl_WhenCodeExists()
    {
        // Arrange
        var shortenUrl = ShortLink.Create(
            "https://www.example.com/test",
            "https://short.url/test123",
            "test123");
        await _store.CreateAsync(shortenUrl, CancellationToken.None);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _store.GetByCodeAsync("test123", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("test123");
        result.LongUrl.Should().Be("https://www.example.com/test");
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenCodeDoesNotExist()
    {
        // Act
        var result = await _store.GetByCodeAsync("nonexistent", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsCodeExistsAsync_ShouldReturnTrue_WhenCodeExists()
    {
        // Arrange
        var shortenUrl = ShortLink.Create(
            "https://www.example.com/test",
            "https://short.url/exists",
            "exists");
        await _store.CreateAsync(shortenUrl, CancellationToken.None);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _store.IsCodeExistsAsync("exists", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCodeExistsAsync_ShouldReturnFalse_WhenCodeDoesNotExist()
    {
        // Act
        var result = await _store.IsCodeExistsAsync("notexists", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldReturnUrl_WhenCodeExists()
    {
        // Arrange
        var shortenUrl = ShortLink.Create(
            "https://www.example.com/original",
            "https://short.url/orig123",
            "orig123");
        await _store.CreateAsync(shortenUrl, CancellationToken.None);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _store.GetOriginalUrlAsync("orig123", CancellationToken.None);

        // Assert
        result.Should().Be("https://www.example.com/original");
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldReturnNull_WhenCodeDoesNotExist()
    {
        // Act
        var result = await _store.GetOriginalUrlAsync("missing", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnShortenUrl_WhenIdExists()
    {
        // Arrange
        var shortenUrl = ShortLink.Create(
            "https://www.example.com/test",
            "https://short.url/id123",
            "id123");
        await _store.CreateAsync(shortenUrl, CancellationToken.None);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _store.GetByIdAsync(shortenUrl.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(shortenUrl.Id);
        result.Code.Should().Be("id123");
    }
}
