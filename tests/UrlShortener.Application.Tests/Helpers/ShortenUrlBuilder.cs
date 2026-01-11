using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Application.Tests.Helpers;

/// <summary>
/// Test data builder for creating ShortenUrl instances in tests
/// </summary>
public class ShortenUrlBuilder
{
    private string _longUrl = "https://www.example.com/test";
    private string _code = "testcode";
    private bool _isExpiring = false;
    private DateTimeOffset? _expiresAt = null;

    public ShortenUrlBuilder WithLongUrl(string longUrl)
    {
        _longUrl = longUrl;
        return this;
    }

    public ShortenUrlBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public ShortenUrlBuilder WithExpiration(DateTimeOffset expiresAt)
    {
        _isExpiring = true;
        _expiresAt = expiresAt;
        return this;
    }

    public ShortenUrlBuilder WithoutExpiration()
    {
        _isExpiring = false;
        _expiresAt = null;
        return this;
    }

    public ShortenUrl Build()
    {
        var shortUrl = $"https://short.url/{_code}";
        return ShortenUrl.Create(_longUrl, shortUrl, _code, _isExpiring, _expiresAt);
    }

    public static ShortenUrlBuilder Default()
    {
        return new ShortenUrlBuilder();
    }
}
