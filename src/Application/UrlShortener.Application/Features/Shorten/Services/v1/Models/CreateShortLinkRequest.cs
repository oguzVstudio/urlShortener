namespace UrlShortener.Application.Features.Shorten.Services.v1.Models;

public record CreateShortLinkRequest(string Url, 
    bool IsExpiring = false, 
    DateTimeOffset? ExpiresAt = null);