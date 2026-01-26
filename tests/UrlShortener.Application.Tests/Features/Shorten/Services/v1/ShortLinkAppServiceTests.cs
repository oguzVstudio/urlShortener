using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using UrlShortener.Application.Features.Shorten.Services.v1;
using UrlShortener.Application.Features.Shorten.Services.v1.Models;
using UrlShortener.Application.Services.Caching;
using UrlShortener.Application.Services.CodeGenerators;
using UrlShortener.Application.Shared.Settings;
using UrlShortener.Application.Tests.Helpers;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Application.Tests.Features.Shorten.Services.v1;

public class ShortLinkAppServiceTests
{
    private readonly Mock<IShortLinkStore> _shortLinkStoreMock;
    private readonly Mock<IShortenBaseDbContext> _contextMock;
    private readonly TestHybridCache _hybridCache;
    private readonly Mock<IDistributedLock> _distributedLockMock;
    private readonly Mock<IUniqueCodeGenerator> _uniqueCodeGeneratorMock;
    private readonly ShortLinkSettings _shortLinkSettings;
    private readonly ShortLinkAppService _service;

    public ShortLinkAppServiceTests()
    {
        _shortLinkStoreMock = new Mock<IShortLinkStore>();
        _contextMock = new Mock<IShortenBaseDbContext>();
        _contextMock.As<ISupportSaveChanges>()
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _hybridCache = new TestHybridCache();
        _distributedLockMock = new Mock<IDistributedLock>();
        _uniqueCodeGeneratorMock = new Mock<IUniqueCodeGenerator>();
        _shortLinkSettings = new ShortLinkSettings { BaseUrl = "https://short.url" };

        var options = Options.Create(_shortLinkSettings);

        _service = new ShortLinkAppService(
            _shortLinkStoreMock.Object,
            _contextMock.Object,
            _hybridCache,
            _distributedLockMock.Object,
            _uniqueCodeGeneratorMock.Object,
            options);
    }

    [Fact]
    public async Task CreateShortLinkAsync_ShouldCreateShortLink_WhenValidRequestProvided()
    {
        // Arrange
        var request = new CreateShortLinkRequest("https://www.example.com/very/long/url");
        var generatedCode = "abc123";
        var cancellationToken = CancellationToken.None;

        _uniqueCodeGeneratorMock
            .Setup(x => x.GenerateAsync(cancellationToken))
            .ReturnsAsync(generatedCode);

        _distributedLockMock
            .Setup(x => x.TryLockAsync(
                It.Is<string>(key => key.Contains(generatedCode)),
                It.IsAny<TimeSpan>(),
                cancellationToken))
            .ReturnsAsync(true);

        _shortLinkStoreMock
            .Setup(x => x.IsCodeExistsAsync(generatedCode, cancellationToken))
            .ReturnsAsync(false);

        _shortLinkStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortLink>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.Is<string>(key => key.Contains(generatedCode)), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateShortLinkAsync(request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ShortUrl.Should().Be($"{_shortLinkSettings.BaseUrl}/{generatedCode}");
        result.Alias.Should().Be(generatedCode);

        _shortLinkStoreMock.Verify(
            x => x.CreateAsync(It.Is<ShortLink>(url =>
                url.LongUrl == request.Url &&
                url.Code == generatedCode &&
                url.ShortUrl == $"{_shortLinkSettings.BaseUrl}/{generatedCode}"),
                cancellationToken),
            Times.Once);

        _distributedLockMock.Verify(
            x => x.TryRemoveAsync(It.Is<string>(key => key.Contains(generatedCode)), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task CreateShortLinkAsync_ShouldCreateExpiringUrl_WhenExpirationIsSet()
    {
        // Arrange
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var request = new CreateShortLinkRequest(
            "https://www.example.com/url",
            IsExpiring: true,
            ExpiresAt: expiresAt);
        var generatedCode = "exp123";
        var cancellationToken = CancellationToken.None;

        _uniqueCodeGeneratorMock
            .Setup(x => x.GenerateAsync(cancellationToken))
            .ReturnsAsync(generatedCode);

        _distributedLockMock
            .Setup(x => x.TryLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), cancellationToken))
            .ReturnsAsync(true);

        _shortLinkStoreMock
            .Setup(x => x.IsCodeExistsAsync(generatedCode, cancellationToken))
            .ReturnsAsync(false);

        _shortLinkStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortLink>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateShortLinkAsync(request, cancellationToken);

        // Assert
        result.Success.Should().BeTrue();
        _shortLinkStoreMock.Verify(
            x => x.CreateAsync(It.Is<ShortLink>(url =>
                url.IsExpiring == true &&
                url.ExpiresAt == expiresAt),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task CreateShortLinkAsync_ShouldRetryCodeGeneration_WhenCodeAlreadyExists()
    {
        // Arrange
        var request = new CreateShortLinkRequest("https://www.example.com/url");
        var firstCode = "code1";
        var secondCode = "code2";
        var cancellationToken = CancellationToken.None;

        var codeSequence = new Queue<string>(new[] { firstCode, secondCode });
        _uniqueCodeGeneratorMock
            .Setup(x => x.GenerateAsync(cancellationToken))
            .ReturnsAsync(() => codeSequence.Dequeue());

        _distributedLockMock
            .Setup(x => x.TryLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), cancellationToken))
            .ReturnsAsync(true);

        _shortLinkStoreMock
            .Setup(x => x.IsCodeExistsAsync(firstCode, cancellationToken))
            .ReturnsAsync(true); // First code exists

        _shortLinkStoreMock
            .Setup(x => x.IsCodeExistsAsync(secondCode, cancellationToken))
            .ReturnsAsync(false); // Second code is available

        _shortLinkStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortLink>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateShortLinkAsync(request, cancellationToken);

        // Assert
        result.Alias.Should().Be(secondCode);
        _uniqueCodeGeneratorMock.Verify(x => x.GenerateAsync(cancellationToken), Times.Exactly(2));
        _shortLinkStoreMock.Verify(x => x.IsCodeExistsAsync(firstCode, cancellationToken), Times.Once);
        _shortLinkStoreMock.Verify(x => x.IsCodeExistsAsync(secondCode, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldReturnOriginalUrl_WhenCodeExists()
    {
        // Arrange
        var code = "abc123";
        var originalUrl = "https://www.example.com/original";
        var cancellationToken = CancellationToken.None;

        _shortLinkStoreMock
            .Setup(x => x.GetOriginalUrlAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalUrl);

        // Act
        var result = await _service.GetOriginalUrlAsync(code, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Found.Should().BeTrue();
        result.OriginalUrl.Should().Be(originalUrl);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldReturnNotFound_WhenCodeDoesNotExist()
    {
        // Arrange
        var code = "nonexistent";
        var cancellationToken = CancellationToken.None;

        _shortLinkStoreMock
            .Setup(x => x.GetOriginalUrlAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.GetOriginalUrlAsync(code, cancellationToken);

        // Assert
        result.Found.Should().BeFalse();
        result.OriginalUrl.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateShortLinkAsync_ShouldRetryCodeGeneration_WhenLockCannotBeAcquired()
    {
        // Arrange
        var request = new CreateShortLinkRequest("https://www.example.com/url");
        var firstCode = "code1";
        var secondCode = "code2";
        var cancellationToken = CancellationToken.None;

        var codeSequence = new Queue<string>(new[] { firstCode, secondCode });
        _uniqueCodeGeneratorMock
            .Setup(x => x.GenerateAsync(cancellationToken))
            .ReturnsAsync(() => codeSequence.Dequeue());

        var lockSequence = new Queue<bool>(new[] { false, true }); // First fails, second succeeds
        _distributedLockMock
            .Setup(x => x.TryLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), cancellationToken))
            .ReturnsAsync(() => lockSequence.Dequeue());

        _shortLinkStoreMock
            .Setup(x => x.IsCodeExistsAsync(secondCode, cancellationToken))
            .ReturnsAsync(false);

        _shortLinkStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortLink>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateShortLinkAsync(request, cancellationToken);

        // Assert
        result.Alias.Should().Be(secondCode);
        _uniqueCodeGeneratorMock.Verify(x => x.GenerateAsync(cancellationToken), Times.Exactly(2));
        _distributedLockMock.Verify(
            x => x.TryLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), cancellationToken),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldUseCache_WhenCalledMultipleTimes()
    {
        // Arrange
        var code = "cached123";
        var originalUrl = "https://www.example.com/original";
        var cancellationToken = CancellationToken.None;

        _shortLinkStoreMock
            .Setup(x => x.GetOriginalUrlAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalUrl);

        // Act - First call
        var result1 = await _service.GetOriginalUrlAsync(code, cancellationToken);
        // Act - Second call (should use cache)
        var result2 = await _service.GetOriginalUrlAsync(code, cancellationToken);

        // Assert
        result1.OriginalUrl.Should().Be(originalUrl);
        result2.OriginalUrl.Should().Be(originalUrl);

        // Store should only be called once due to caching
        _shortLinkStoreMock.Verify(
            x => x.GetOriginalUrlAsync(code, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateShortLinkAsync_ShouldRemoveLock_AfterSuccessfulCreation()
    {
        // Arrange
        var request = new CreateShortLinkRequest("https://www.example.com/url");
        var generatedCode = "abc123";
        var cancellationToken = CancellationToken.None;

        _uniqueCodeGeneratorMock
            .Setup(x => x.GenerateAsync(cancellationToken))
            .ReturnsAsync(generatedCode);

        _distributedLockMock
            .Setup(x => x.TryLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), cancellationToken))
            .ReturnsAsync(true);

        _shortLinkStoreMock
            .Setup(x => x.IsCodeExistsAsync(generatedCode, cancellationToken))
            .ReturnsAsync(false);

        _shortLinkStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortLink>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.Is<string>(key => key.Contains(generatedCode)), cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _service.CreateShortLinkAsync(request, cancellationToken);

        // Assert
        _distributedLockMock.Verify(
            x => x.TryRemoveAsync(It.Is<string>(key => key.Contains(generatedCode)), cancellationToken),
            Times.Once,
            "Lock should be removed after successful creation");
    }

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("https://github.com/user/repo")]
    [InlineData("https://stackoverflow.com/questions/123456")]
    public async Task CreateShortLinkAsync_ShouldHandleDifferentUrls(string longUrl)
    {
        // Arrange
        var request = new CreateShortLinkRequest(longUrl);
        var generatedCode = "test123";
        var cancellationToken = CancellationToken.None;

        _uniqueCodeGeneratorMock
            .Setup(x => x.GenerateAsync(cancellationToken))
            .ReturnsAsync(generatedCode);

        _distributedLockMock
            .Setup(x => x.TryLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), cancellationToken))
            .ReturnsAsync(true);

        _shortLinkStoreMock
            .Setup(x => x.IsCodeExistsAsync(generatedCode, cancellationToken))
            .ReturnsAsync(false);

        _shortLinkStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortLink>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateShortLinkAsync(request, cancellationToken);

        // Assert
        result.Success.Should().BeTrue();
        _shortLinkStoreMock.Verify(
            x => x.CreateAsync(It.Is<ShortLink>(link => link.LongUrl == longUrl), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task CreateShortLinkAsync_ShouldSaveChanges_WhenContextSupportsSaveChanges()
    {
        // Arrange
        var request = new CreateShortLinkRequest("https://www.example.com/url");
        var generatedCode = "abc123";
        var cancellationToken = CancellationToken.None;

        _uniqueCodeGeneratorMock
            .Setup(x => x.GenerateAsync(cancellationToken))
            .ReturnsAsync(generatedCode);

        _distributedLockMock
            .Setup(x => x.TryLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), cancellationToken))
            .ReturnsAsync(true);

        _shortLinkStoreMock
            .Setup(x => x.IsCodeExistsAsync(generatedCode, cancellationToken))
            .ReturnsAsync(false);

        _shortLinkStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortLink>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _service.CreateShortLinkAsync(request, cancellationToken);

        // Assert
        _contextMock.As<ISupportSaveChanges>().Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once,
            "SaveChangesAsync should be called to persist changes");
    }
}
