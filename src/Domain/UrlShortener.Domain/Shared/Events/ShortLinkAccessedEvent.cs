namespace UrlShortener.Domain.Shared.Events;

public class ShortLinkAccessedEvent
{
    public string Code { get; set; } = default!;
    public string IpAddress { get; set; } = default!;
    public string UserAgent { get; set; } = default!;
    public DateTimeOffset AccessedAt { get; set; }
}