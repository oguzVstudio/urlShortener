namespace UrlShortener.Application.Features.Shorten.Services.v1.Models;

public record CreateShortUrlResponse(string ShortUrl, 
    string Alias, 
    bool Success);