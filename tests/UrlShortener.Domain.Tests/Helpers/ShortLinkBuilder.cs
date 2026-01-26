using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Domain.Tests.Helpers;

/// <summary>
/// Test data builder for creating ShortLink instances in tests
/// </summary>
public class ShortLinkBuilder
{
    private string _longUrl = "https://www.example.com/test";
    private string _code = "testcode";
    private bool _isExpiring = false;
    private DateTimeOffset? _expiresAt = null;

    public ShortLinkBuilder WithLongUrl(string longUrl)
    {
        _longUrl = longUrl;
        return this;
    }

    public ShortLinkBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public ShortLinkBuilder WithExpiration(DateTimeOffset expiresAt)
    {
        _isExpiring = true;
        _expiresAt = expiresAt;
        return this;
    }

    public ShortLinkBuilder WithoutExpiration()
    {
        _isExpiring = false;
        _expiresAt = null;
        return this;
    }

    public ShortLink Build()
    {
        var shortUrl = $"https://short.url/{_code}";
        return ShortLink.Create(_longUrl, shortUrl, _code, _isExpiring, _expiresAt);
    }

    public static ShortLinkBuilder Default()
    {
        return new ShortLinkBuilder();
    }
}
