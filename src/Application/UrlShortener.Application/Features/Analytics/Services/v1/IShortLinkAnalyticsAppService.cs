using UrlShortener.Application.Features.Analytics.Services.v1.Models;

namespace UrlShortener.Application.Features.Analytics.Services.v1;

public interface IShortLinkAnalyticsAppService
{
    Task<bool> CreateShortLinkAccessLogAsync(CreateShortLinkAccessLogRequest request, CancellationToken cancellationToken);
}