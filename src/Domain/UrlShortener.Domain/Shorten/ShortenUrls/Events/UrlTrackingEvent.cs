namespace UrlShortener.Domain.Shorten.ShortenUrls.Events;

public class UrlTrackingEvent
{
    public string Code { get; set; } = default!;
    public string IpAddress { get; set; } = default!;
    public string UserAgent { get; set; } = default!;
    public DateTimeOffset AccessedAt { get; set; }
}