using UrlShortener.Application.Features.Shorten.Services.v1.Models;

namespace UrlShortener.Application.Features.Shorten.Services.v1;

public interface IShortenUrlAppService
{
    Task<CreateShortUrlResponse> ShortenUrlAsync(CreateShortUrlRequest request, CancellationToken cancellationToken);
    Task<GetOriginalUrlResponse> GetOriginalUrlAsync(string code, CancellationToken cancellationToken);
    Task<bool> TrackUrlAccessAsync(CreateShortUrlTrackRequest request, CancellationToken cancellationToken);
}