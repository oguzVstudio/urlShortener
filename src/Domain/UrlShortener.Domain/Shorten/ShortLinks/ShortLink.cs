namespace UrlShortener.Domain.Shorten.ShortLinks;

public class ShortLink
{
    public Guid Id { get; private set; }
    public string LongUrl { get; private set; } = string.Empty;
    public string ShortUrl { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public bool IsExpiring { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    private ShortLink()
    {
    }

    public static ShortLink Create(
        string longUrl, 
        string shortUrl, 
        string code,
        bool isExpiring = false,
        DateTimeOffset? expiresAt = null)
    {
        return new ShortLink
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
    
    public bool IsExpired =>
        IsExpiring && ExpiresAt.HasValue && DateTimeOffset.UtcNow >= ExpiresAt.Value;
}