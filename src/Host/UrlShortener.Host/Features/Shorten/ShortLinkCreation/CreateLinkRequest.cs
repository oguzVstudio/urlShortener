namespace UrlShortener.Host.Features.Shorten.ShortLinkCreation;

public class CreateLinkRequest
{
    public string Url { get; set; } = default!;
    public bool IsExpiring { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}