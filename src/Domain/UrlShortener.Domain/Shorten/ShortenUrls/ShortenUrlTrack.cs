namespace UrlShortener.Domain.Shorten.ShortenUrls;

public class ShortenUrlTrack
{
    public Guid Id { get; private set; }
    public Guid ShortenUrlId { get; private set; }
    public string Code { get; private set; } = default!;
    public string IpAddress { get; private set; } = default!;
    public string UserAgent { get; private set; } = default!;
    public DateTimeOffset AccessedAt { get; private set; }
    public DateTimeOffset CreatedOnUtc { get; private set; }

    private ShortenUrlTrack() { }

    public static ShortenUrlTrack Create(
        Guid shortenUrlId,
        string code,
        string ipAddress,
        string userAgent,
        DateTimeOffset accessedAt)
    {
        return new ShortenUrlTrack
        {
            Id = Guid.NewGuid(),
            ShortenUrlId = shortenUrlId,
            Code = code,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AccessedAt = accessedAt,
            CreatedOnUtc = DateTimeOffset.UtcNow
        };
    }
}