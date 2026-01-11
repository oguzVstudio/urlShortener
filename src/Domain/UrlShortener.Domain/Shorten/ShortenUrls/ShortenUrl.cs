namespace UrlShortener.Domain.Shorten.ShortenUrls;

public class ShortenUrl
{
    public Guid Id { get; private set; }
    public string LongUrl { get; private set; } = string.Empty;
    public string ShortUrl { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public bool IsExpiring { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public int AttemptCount { get; private set; }

    private ShortenUrl()
    {
    }

    public static ShortenUrl Create(
        string longUrl, 
        string shortUrl, 
        string code,
        bool isExpiring = false,
        DateTimeOffset? expiresAt = null)
    {
        return new ShortenUrl
        {
            Id = Guid.NewGuid(),
            LongUrl = longUrl,
            ShortUrl = shortUrl,
            Code = code,
            IsExpiring = isExpiring,
            ExpiresAt = expiresAt,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    public void IncrementAttemptCount()
    {
        AttemptCount++;
    }
}