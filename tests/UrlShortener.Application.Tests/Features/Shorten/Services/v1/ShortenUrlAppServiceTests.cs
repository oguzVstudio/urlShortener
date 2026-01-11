using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using UrlShortener.Application.Features.Shorten.Services.v1;
using UrlShortener.Application.Features.Shorten.Services.v1.Models;
using UrlShortener.Application.Services.CodeGenerators;
using UrlShortener.Application.Shared.Cache;
using UrlShortener.Application.Tests.Helpers;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.Settings;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Application.Tests.Features.Shorten.Services.v1;

public class ShortenUrlAppServiceTests
{
    private readonly Mock<IShortenUrlStore> _shortenUrlStoreMock;
    private readonly Mock<IShortenUrlTrackStore> _shortenUrlTrackStoreMock;
    private readonly Mock<IShortenBaseDbContext> _contextMock;
    private readonly TestHybridCache _hybridCache;
    private readonly Mock<IDistributedLock> _distributedLockMock;
    private readonly Mock<IUniqueCodeGenerator> _uniqueCodeGeneratorMock;
    private readonly ShortenUrlSettings _shortenUrlSettings;
    private readonly ShortenUrlAppService _service;

    public ShortenUrlAppServiceTests()
    {
        _shortenUrlStoreMock = new Mock<IShortenUrlStore>();
        _shortenUrlTrackStoreMock = new Mock<IShortenUrlTrackStore>();
        _contextMock = new Mock<IShortenBaseDbContext>();
        _contextMock.As<ISupportSaveChanges>()
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _hybridCache = new TestHybridCache();
        _distributedLockMock = new Mock<IDistributedLock>();
        _uniqueCodeGeneratorMock = new Mock<IUniqueCodeGenerator>();
        _shortenUrlSettings = new ShortenUrlSettings { BaseUrl = "https://short.url" };

        var options = Options.Create(_shortenUrlSettings);

        _service = new ShortenUrlAppService(
            _shortenUrlStoreMock.Object,
            _shortenUrlTrackStoreMock.Object,
            _contextMock.Object,
            _hybridCache,
            _distributedLockMock.Object,
            _uniqueCodeGeneratorMock.Object,
            options);
    }

    [Fact]
    public async Task ShortenUrlAsync_ShouldCreateShortUrl_WhenValidRequestProvided()
    {
        // Arrange
        var request = new CreateShortUrlRequest("https://www.example.com/very/long/url");
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

        _shortenUrlStoreMock
            .Setup(x => x.IsCodeExistsAsync(generatedCode, cancellationToken))
            .ReturnsAsync(false);

        _shortenUrlStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortenUrl>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.Is<string>(key => key.Contains(generatedCode)), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ShortenUrlAsync(request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ShortUrl.Should().Be($"{_shortenUrlSettings.BaseUrl}/{generatedCode}");
        result.Alias.Should().Be(generatedCode);

        _shortenUrlStoreMock.Verify(
            x => x.CreateAsync(It.Is<ShortenUrl>(url =>
                url.LongUrl == request.Url &&
                url.Code == generatedCode &&
                url.ShortUrl == $"{_shortenUrlSettings.BaseUrl}/{generatedCode}"),
                cancellationToken),
            Times.Once);

        _distributedLockMock.Verify(
            x => x.TryRemoveAsync(It.Is<string>(key => key.Contains(generatedCode)), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ShortenUrlAsync_ShouldCreateExpiringUrl_WhenExpirationIsSet()
    {
        // Arrange
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var request = new CreateShortUrlRequest(
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

        _shortenUrlStoreMock
            .Setup(x => x.IsCodeExistsAsync(generatedCode, cancellationToken))
            .ReturnsAsync(false);

        _shortenUrlStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortenUrl>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ShortenUrlAsync(request, cancellationToken);

        // Assert
        result.Success.Should().BeTrue();
        _shortenUrlStoreMock.Verify(
            x => x.CreateAsync(It.Is<ShortenUrl>(url =>
                url.IsExpiring == true &&
                url.ExpiresAt == expiresAt),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ShortenUrlAsync_ShouldRetryCodeGeneration_WhenCodeAlreadyExists()
    {
        // Arrange
        var request = new CreateShortUrlRequest("https://www.example.com/url");
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

        _shortenUrlStoreMock
            .Setup(x => x.IsCodeExistsAsync(firstCode, cancellationToken))
            .ReturnsAsync(true); // First code exists

        _shortenUrlStoreMock
            .Setup(x => x.IsCodeExistsAsync(secondCode, cancellationToken))
            .ReturnsAsync(false); // Second code is available

        _shortenUrlStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortenUrl>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _distributedLockMock
            .Setup(x => x.TryRemoveAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ShortenUrlAsync(request, cancellationToken);

        // Assert
        result.Alias.Should().Be(secondCode);
        _uniqueCodeGeneratorMock.Verify(x => x.GenerateAsync(cancellationToken), Times.Exactly(2));
        _shortenUrlStoreMock.Verify(x => x.IsCodeExistsAsync(firstCode, cancellationToken), Times.Once);
        _shortenUrlStoreMock.Verify(x => x.IsCodeExistsAsync(secondCode, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldReturnOriginalUrl_WhenCodeExists()
    {
        // Arrange
        var code = "abc123";
        var originalUrl = "https://www.example.com/original";
        var cancellationToken = CancellationToken.None;

        _shortenUrlStoreMock
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

        _shortenUrlStoreMock
            .Setup(x => x.GetOriginalUrlAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.GetOriginalUrlAsync(code, cancellationToken);

        // Assert
        result.Found.Should().BeFalse();
        result.OriginalUrl.Should().BeEmpty();
    }

    [Fact]
    public async Task TrackUrlAccessAsync_ShouldTrackAccess_WhenCodeExists()
    {
        // Arrange
        var shortenUrlId = Guid.NewGuid();
        var code = "abc123";
        var request = new CreateShortUrlTrackRequest(
            code,
            "Mozilla/5.0",
            "192.168.1.1",
            DateTimeOffset.UtcNow);
        var cancellationToken = CancellationToken.None;

        var shortenUrl = ShortenUrl.Create(
            "https://www.example.com/url",
            $"https://short.url/{code}",
            code);

        _shortenUrlStoreMock
            .Setup(x => x.GetByCodeAsync(code, cancellationToken))
            .ReturnsAsync(shortenUrl);

        _shortenUrlTrackStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortenUrlTrack>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.TrackUrlAccessAsync(request, cancellationToken);

        // Assert
        result.Should().BeTrue();
        shortenUrl.AttemptCount.Should().Be(1);

        _shortenUrlTrackStoreMock.Verify(
            x => x.CreateAsync(It.Is<ShortenUrlTrack>(track =>
                track.Code == code &&
                track.IpAddress == request.IpAddress &&
                track.UserAgent == request.UserAgent),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task TrackUrlAccessAsync_ShouldReturnFalse_WhenCodeDoesNotExist()
    {
        // Arrange
        var code = "nonexistent";
        var request = new CreateShortUrlTrackRequest(
            code,
            "Mozilla/5.0",
            "192.168.1.1",
            DateTimeOffset.UtcNow);
        var cancellationToken = CancellationToken.None;

        _shortenUrlStoreMock
            .Setup(x => x.GetByCodeAsync(code, cancellationToken))
            .ReturnsAsync((ShortenUrl?)null);

        // Act
        var result = await _service.TrackUrlAccessAsync(request, cancellationToken);

        // Assert
        result.Should().BeFalse();
        _shortenUrlTrackStoreMock.Verify(
            x => x.CreateAsync(It.IsAny<ShortenUrlTrack>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TrackUrlAccessAsync_ShouldIncrementAttemptCount()
    {
        // Arrange
        var code = "abc123";
        var request = new CreateShortUrlTrackRequest(
            code,
            "Mozilla/5.0",
            "192.168.1.1",
            DateTimeOffset.UtcNow);
        var cancellationToken = CancellationToken.None;

        var shortenUrl = ShortenUrl.Create(
            "https://www.example.com/url",
            $"https://short.url/{code}",
            code);

        // Simulate previous attempts
        shortenUrl.IncrementAttemptCount();
        shortenUrl.IncrementAttemptCount();

        _shortenUrlStoreMock
            .Setup(x => x.GetByCodeAsync(code, cancellationToken))
            .ReturnsAsync(shortenUrl);

        _shortenUrlTrackStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortenUrlTrack>(), cancellationToken))
            .Returns(Task.CompletedTask);

        var initialCount = shortenUrl.AttemptCount;

        // Act
        await _service.TrackUrlAccessAsync(request, cancellationToken);

        // Assert
        shortenUrl.AttemptCount.Should().Be(initialCount + 1);
    }
}
