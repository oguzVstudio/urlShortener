using FluentAssertions;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Domain.Tests.Shorten.ShortenUrls;

public class ShortenUrlTrackTests
{
    [Fact]
    public void Create_ShouldCreateValidShortenUrlTrack_WhenValidParametersProvided()
    {
        // Arrange
        var shortenUrlId = Guid.NewGuid();
        var code = "abc123";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var accessedAt = DateTimeOffset.UtcNow;

        // Act
        var track = ShortenUrlTrack.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);

        // Assert
        track.Should().NotBeNull();
        track.Id.Should().NotBeEmpty();
        track.ShortenUrlId.Should().Be(shortenUrlId);
        track.Code.Should().Be(code);
        track.IpAddress.Should().Be(ipAddress);
        track.UserAgent.Should().Be(userAgent);
        track.AccessedAt.Should().Be(accessedAt);
        track.CreatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId_ForEachInstance()
    {
        // Arrange
        var shortenUrlId = Guid.NewGuid();
        var code = "test";
        var ipAddress = "192.168.1.1";
        var userAgent = "TestAgent";
        var accessedAt = DateTimeOffset.UtcNow;

        // Act
        var track1 = ShortenUrlTrack.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);
        var track2 = ShortenUrlTrack.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);

        // Assert
        track1.Id.Should().NotBe(track2.Id);
    }

    [Theory]
    [InlineData("192.168.1.1", "Mozilla/5.0")]
    [InlineData("10.0.0.1", "Chrome/96.0")]
    [InlineData("172.16.0.1", "Safari/15.0")]
    public void Create_ShouldHandleDifferentIpAndUserAgents(string ipAddress, string userAgent)
    {
        // Arrange
        var shortenUrlId = Guid.NewGuid();
        var code = "test";
        var accessedAt = DateTimeOffset.UtcNow;

        // Act
        var track = ShortenUrlTrack.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);

        // Assert
        track.IpAddress.Should().Be(ipAddress);
        track.UserAgent.Should().Be(userAgent);
    }

    [Fact]
    public void Create_ShouldPreserveAccessedAtTime()
    {
        // Arrange
        var shortenUrlId = Guid.NewGuid();
        var code = "test";
        var ipAddress = "192.168.1.1";
        var userAgent = "TestAgent";
        var accessedAt = DateTimeOffset.UtcNow.AddHours(-2); // 2 hours ago

        // Act
        var track = ShortenUrlTrack.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);

        // Assert
        track.AccessedAt.Should().Be(accessedAt);
        track.CreatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
