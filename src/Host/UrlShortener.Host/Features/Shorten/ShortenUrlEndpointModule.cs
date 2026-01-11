using UrlShortener.Host.Features.Shorten.GettingShortenUrl;
using UrlShortener.Host.Features.Shorten.ShortenUrlCreation;

namespace UrlShortener.Host.Features.Shorten;

public class ShortenUrlEndpointModule : IEndpointModule
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/urls")
            .WithTags("Url");

        group.MapShortenUrlEndpoint()
            .MapGetOriginalUrlEndpoint();
    }
}