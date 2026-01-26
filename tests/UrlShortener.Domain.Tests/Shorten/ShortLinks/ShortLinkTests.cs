using FluentAssertions;
using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Domain.Tests.Shorten.ShortLinks;

public class ShortLinkTests
{
    [Fact]
    public void Create_ShouldCreateValidShortenUrl_WhenValidParametersProvided()
    {
        // Arrange
        var longUrl = "https://www.example.com/very/long/url";
        var shortUrl = "https://short.url/abc123";
        var code = "abc123";
        var isExpiring = false;
        DateTimeOffset? expiresAt = null;

        // Act
        var shortenUrl = ShortLink.Create(longUrl, shortUrl, code, isExpiring, expiresAt);

        // Assert
        shortenUrl.Should().NotBeNull();
        shortenUrl.Id.Should().NotBeEmpty();
        shortenUrl.LongUrl.Should().Be(longUrl);
        shortenUrl.ShortUrl.Should().Be(shortUrl);
        shortenUrl.Code.Should().Be(code);
        shortenUrl.IsExpiring.Should().BeFalse();
        shortenUrl.ExpiresAt.Should().BeNull();
        shortenUrl.CreatedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldCreateExpiringUrl_WhenExpirationIsSet()
    {
        // Arrange
        var longUrl = "https://www.example.com/very/long/url";
        var shortUrl = "https://short.url/xyz789";
        var code = "xyz789";
        var isExpiring = true;
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var shortenUrl = ShortLink.Create(longUrl, shortUrl, code, isExpiring, expiresAt);

        // Assert
        shortenUrl.IsExpiring.Should().BeTrue();
        shortenUrl.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId_ForEachInstance()
    {
        // Arrange
        var longUrl = "https://www.example.com/url";
        var shortUrl = "https://short.url/test";
        var code = "test";

        // Act
        var shortenUrl1 = ShortLink.Create(longUrl, shortUrl, code);
        var shortenUrl2 = ShortLink.Create(longUrl, shortUrl, code);

        // Assert
        shortenUrl1.Id.Should().NotBe(shortenUrl2.Id);
    }

    [Theory]
    [InlineData("https://www.google.com", "https://short.url/ggl", "ggl")]
    [InlineData("https://www.microsoft.com/products", "https://short.url/msft", "msft")]
    [InlineData("https://github.com/user/repo/issues/123", "https://short.url/gh123", "gh123")]
    public void Create_ShouldHandleDifferentUrls_Successfully(string longUrl, string shortUrl, string code)
    {
        // Act
        var shortenUrl = ShortLink.Create(longUrl, shortUrl, code);

        // Assert
        shortenUrl.LongUrl.Should().Be(longUrl);
        shortenUrl.ShortUrl.Should().Be(shortUrl);
        shortenUrl.Code.Should().Be(code);
    }

    [Fact]
    public void Create_WithDefaultParameters_ShouldNotExpire()
    {
        // Arrange & Act
        var shortenUrl = ShortLink.Create(
            "https://www.example.com/url",
            "https://short.url/test",
            "test");

        // Assert
        shortenUrl.IsExpiring.Should().BeFalse();
        shortenUrl.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenUrlDoesNotExpire()
    {
        // Arrange
        var shortLink = ShortLink.Create(
            "https://www.example.com/url",
            "https://short.url/test",
            "test",
            isExpiring: false);

        // Act & Assert
        shortLink.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenExpirationIsInFuture()
    {
        // Arrange
        var futureDate = DateTimeOffset.UtcNow.AddDays(7);
        var shortLink = ShortLink.Create(
            "https://www.example.com/url",
            "https://short.url/test",
            "test",
            isExpiring: true,
            expiresAt: futureDate);

        // Act & Assert
        shortLink.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenExpirationIsInPast()
    {
        // Arrange
        var pastDate = DateTimeOffset.UtcNow.AddDays(-1);
        var shortLink = ShortLink.Create(
            "https://www.example.com/url",
            "https://short.url/test",
            "test",
            isExpiring: true,
            expiresAt: pastDate);

        // Act & Assert
        shortLink.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenExpiresAtIsNull()
    {
        // Arrange
        var shortLink = ShortLink.Create(
            "https://www.example.com/url",
            "https://short.url/test",
            "test",
            isExpiring: true,
            expiresAt: null);

        // Act & Assert
        shortLink.IsExpired.Should().BeFalse();
    }
}
