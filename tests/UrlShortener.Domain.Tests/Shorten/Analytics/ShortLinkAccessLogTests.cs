using FluentAssertions;
using UrlShortener.Domain.Shorten.Analytics;

namespace UrlShortener.Domain.Tests.Shorten.Analytics;

public class ShortLinkAccessLogTests
{
    [Fact]
    public void Create_ShouldCreateValidShortLinkAccessLog_WhenValidParametersProvided()
    {
        // Arrange
        var shortenUrlId = Guid.NewGuid();
        var code = "abc123";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var accessedAt = DateTimeOffset.UtcNow;

        // Act
        var accessLog = ShortLinkAccessLog.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);

        // Assert
        accessLog.Should().NotBeNull();
        accessLog.Id.Should().NotBeEmpty();
        accessLog.ShortLinkId.Should().Be(shortenUrlId);
        accessLog.Code.Should().Be(code);
        accessLog.IpAddress.Should().Be(ipAddress);
        accessLog.UserAgent.Should().Be(userAgent);
        accessLog.AccessedAt.Should().Be(accessedAt);
        accessLog.CreatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
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
        var accessLog1 = ShortLinkAccessLog.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);
        var accessLog2 = ShortLinkAccessLog.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);

        // Assert
        accessLog1.Id.Should().NotBe(accessLog2.Id);
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
        var accessLog = ShortLinkAccessLog.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);

        // Assert
        accessLog.IpAddress.Should().Be(ipAddress);
        accessLog.UserAgent.Should().Be(userAgent);
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
        var accessLog = ShortLinkAccessLog.Create(shortenUrlId, code, ipAddress, userAgent, accessedAt);

        // Assert
        accessLog.AccessedAt.Should().Be(accessedAt);
        accessLog.CreatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
