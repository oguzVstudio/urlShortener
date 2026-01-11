namespace UrlShortener.Application.Features.Shorten.Services.v1.Models;

public record GetOriginalUrlResponse(string? OriginalUrl, bool Found);