namespace UrlShortener.Application.Features.Analytics.Services.v1.Models;

public record CreateShortLinkAccessLogRequest(
    string Code,
    string UserAgent,
    string IpAddress,
    DateTimeOffset AccessedAt);