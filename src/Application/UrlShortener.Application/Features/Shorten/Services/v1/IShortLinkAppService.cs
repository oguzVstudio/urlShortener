using UrlShortener.Application.Features.Shorten.Services.v1.Models;

namespace UrlShortener.Application.Features.Shorten.Services.v1;

public interface IShortLinkAppService
{
    Task<CreateShortLinkResponse> CreateShortLinkAsync(CreateShortLinkRequest request, CancellationToken cancellationToken);
    Task<GetOriginalUrlResponse> GetOriginalUrlAsync(string code, CancellationToken cancellationToken);
}