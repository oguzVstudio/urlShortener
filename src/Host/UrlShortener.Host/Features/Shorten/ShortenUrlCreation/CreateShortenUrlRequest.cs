namespace UrlShortener.Host.Features.Shorten.ShortenUrlCreation;

public class CreateShortenUrlRequest
{
    public string Url { get; set; } = default!;
    public bool IsExpiring { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}