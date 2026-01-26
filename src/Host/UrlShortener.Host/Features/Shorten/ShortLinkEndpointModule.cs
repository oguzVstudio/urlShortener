using UrlShortener.Host.Features.Shorten.GettingShortLink;
using UrlShortener.Host.Features.Shorten.ShortLinkCreation;

namespace UrlShortener.Host.Features.Shorten;

public class ShortLinkEndpointModule : IEndpointModule
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/urls")
            .WithTags("Url");

        group.MapShortLinkEndpoint()
            .MapGetOriginalUrlEndpoint();
    }
}