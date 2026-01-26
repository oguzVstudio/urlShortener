using FluentAssertions;
using Moq;
using UrlShortener.Application.Features.Analytics.Services.v1;
using UrlShortener.Application.Features.Analytics.Services.v1.Models;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten;
using UrlShortener.Domain.Shorten.Analytics;
using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Application.Tests.Features.Analytics.Services.v1;

public class ShortLinkAnalyticsAppServiceTests
{
    private readonly Mock<IShortLinkStore> _shortLinkStoreMock;
    private readonly Mock<IShortLinkAccessLogStore> _shortLinkAccessLogStoreMock;
    private readonly Mock<IShortenBaseDbContext> _contextMock;
    private readonly ShortLinkAnalyticsAppService _service;

    public ShortLinkAnalyticsAppServiceTests()
    {
        _shortLinkStoreMock = new Mock<IShortLinkStore>();
        _shortLinkAccessLogStoreMock = new Mock<IShortLinkAccessLogStore>();
        _contextMock = new Mock<IShortenBaseDbContext>();
        _contextMock.As<ISupportSaveChanges>()
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _service = new ShortLinkAnalyticsAppService(
            _shortLinkStoreMock.Object,
            _shortLinkAccessLogStoreMock.Object,
            _contextMock.Object);
    }

    [Fact]
    public async Task CreateShortLinkAccessLogAsync_ShouldCreateAccessLog_WhenShortLinkExists()
    {
        // Arrange
        var shortLinkId = Guid.NewGuid();
        var code = "abc123";
        var shortLink = ShortLink.Create(
            "https://www.example.com/long",
            "https://short.url/abc123",
            code);

        // Use reflection to set the Id property
        typeof(ShortLink).GetProperty("Id")!
            .SetValue(shortLink, shortLinkId);

        var request = new CreateShortLinkAccessLogRequest(
            Code: code,
            UserAgent: "Mozilla/5.0",
            IpAddress: "192.168.1.1",
            AccessedAt: DateTimeOffset.UtcNow);

        var cancellationToken = CancellationToken.None;

        _shortLinkStoreMock
            .Setup(x => x.GetByCodeAsync(code, cancellationToken))
            .ReturnsAsync(shortLink);

        _shortLinkAccessLogStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortLinkAccessLog>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateShortLinkAccessLogAsync(request, cancellationToken);

        // Assert
        result.Should().BeTrue();

        _shortLinkAccessLogStoreMock.Verify(
            x => x.CreateAsync(
                It.Is<ShortLinkAccessLog>(log =>
                    log.ShortLinkId == shortLinkId &&
                    log.Code == code &&
                    log.IpAddress == request.IpAddress &&
                    log.UserAgent == request.UserAgent &&
                    log.AccessedAt == request.AccessedAt),
                cancellationToken),
            Times.Once);

        _contextMock.As<ISupportSaveChanges>().Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task CreateShortLinkAccessLogAsync_ShouldReturnFalse_WhenShortLinkDoesNotExist()
    {
        // Arrange
        var code = "nonexistent";
        var request = new CreateShortLinkAccessLogRequest(
            Code: code,
            UserAgent: "Mozilla/5.0",
            IpAddress: "192.168.1.1",
            AccessedAt: DateTimeOffset.UtcNow);

        var cancellationToken = CancellationToken.None;

        _shortLinkStoreMock
            .Setup(x => x.GetByCodeAsync(code, cancellationToken))
            .ReturnsAsync((ShortLink?)null);

        // Act
        var result = await _service.CreateShortLinkAccessLogAsync(request, cancellationToken);

        // Assert
        result.Should().BeFalse();

        _shortLinkAccessLogStoreMock.Verify(
            x => x.CreateAsync(It.IsAny<ShortLinkAccessLog>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _contextMock.As<ISupportSaveChanges>().Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateShortLinkAccessLogAsync_ShouldHandleDifferentUserAgents()
    {
        // Arrange
        var shortLinkId = Guid.NewGuid();
        var code = "test123";
        var shortLink = ShortLink.Create(
            "https://www.example.com/long",
            "https://short.url/test123",
            code);

        typeof(ShortLink).GetProperty("Id")!
            .SetValue(shortLink, shortLinkId);

        var userAgents = new[]
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/96.0",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) Safari/15.0",
            "PostmanRuntime/7.28.4"
        };

        var cancellationToken = CancellationToken.None;

        _shortLinkStoreMock
            .Setup(x => x.GetByCodeAsync(code, cancellationToken))
            .ReturnsAsync(shortLink);

        foreach (var userAgent in userAgents)
        {
            // Arrange
            var request = new CreateShortLinkAccessLogRequest(
                Code: code,
                UserAgent: userAgent,
                IpAddress: "192.168.1.1",
                AccessedAt: DateTimeOffset.UtcNow);

            _shortLinkAccessLogStoreMock
                .Setup(x => x.CreateAsync(It.IsAny<ShortLinkAccessLog>(), cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateShortLinkAccessLogAsync(request, cancellationToken);

            // Assert
            result.Should().BeTrue();
            _shortLinkAccessLogStoreMock.Verify(
                x => x.CreateAsync(
                    It.Is<ShortLinkAccessLog>(log => log.UserAgent == userAgent),
                    cancellationToken),
                Times.Once);
        }
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    public async Task CreateShortLinkAccessLogAsync_ShouldHandleDifferentIpAddresses(string ipAddress)
    {
        // Arrange
        var shortLinkId = Guid.NewGuid();
        var code = "test123";
        var shortLink = ShortLink.Create(
            "https://www.example.com/long",
            "https://short.url/test123",
            code);

        typeof(ShortLink).GetProperty("Id")!
            .SetValue(shortLink, shortLinkId);

        var request = new CreateShortLinkAccessLogRequest(
            Code: code,
            UserAgent: "Mozilla/5.0",
            IpAddress: ipAddress,
            AccessedAt: DateTimeOffset.UtcNow);

        var cancellationToken = CancellationToken.None;

        _shortLinkStoreMock
            .Setup(x => x.GetByCodeAsync(code, cancellationToken))
            .ReturnsAsync(shortLink);

        _shortLinkAccessLogStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<ShortLinkAccessLog>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateShortLinkAccessLogAsync(request, cancellationToken);

        // Assert
        result.Should().BeTrue();
        _shortLinkAccessLogStoreMock.Verify(
            x => x.CreateAsync(
                It.Is<ShortLinkAccessLog>(log => log.IpAddress == ipAddress),
                cancellationToken),
            Times.Once);
    }
}
