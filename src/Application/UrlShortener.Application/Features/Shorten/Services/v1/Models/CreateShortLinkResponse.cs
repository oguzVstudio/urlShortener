namespace UrlShortener.Application.Features.Shorten.Services.v1.Models;

public record CreateShortLinkResponse(string ShortUrl, 
    string Alias, 
    bool Success);