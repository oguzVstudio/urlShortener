namespace UrlShortener.Application.Features.Shorten.Services.v1.Models;

public record CreateShortUrlRequest(string Url, 
    bool IsExpiring = false, 
    DateTimeOffset? ExpiresAt = null);