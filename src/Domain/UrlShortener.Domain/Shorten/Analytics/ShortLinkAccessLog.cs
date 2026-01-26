namespace UrlShortener.Domain.Shorten.Analytics;

public class ShortLinkAccessLog
{
    public Guid Id { get; private set; }
    public Guid ShortLinkId { get; private set; }
    public string Code { get; private set; } = default!;
    public string IpAddress { get; private set; } = default!;
    public string UserAgent { get; private set; } = default!;
    public DateTimeOffset AccessedAt { get; private set; }
    public DateTimeOffset CreatedOnUtc { get; private set; }

    private ShortLinkAccessLog() { }

    public static ShortLinkAccessLog Create(
        Guid shortLinkId,
        string code,
        string ipAddress,
        string userAgent,
        DateTimeOffset accessedAt)
    {
        return new ShortLinkAccessLog
        {
            Id = Guid.NewGuid(),
            ShortLinkId = shortLinkId,
            Code = code,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AccessedAt = accessedAt,
            CreatedOnUtc = DateTimeOffset.UtcNow
        };
    }
}