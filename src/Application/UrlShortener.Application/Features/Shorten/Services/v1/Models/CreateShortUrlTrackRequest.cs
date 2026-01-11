namespace UrlShortener.Application.Features.Shorten.Services.v1.Models;

public record CreateShortUrlTrackRequest(
    string Code, 
    string UserAgent, 
    string IpAddress, 
    DateTimeOffset AccessedAt);