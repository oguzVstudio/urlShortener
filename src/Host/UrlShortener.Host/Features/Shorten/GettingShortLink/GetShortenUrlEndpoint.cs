using UrlShortener.Application.Features.Shorten.Services.v1;

namespace UrlShortener.Host.Features.Shorten.GettingShortLink;

public static partial class ShortLinkEndpoints
{
    public static RouteGroupBuilder MapGetOriginalUrlEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{code}", HandleAsync)
            .WithName("Get Original Url")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireRateLimiting("fixed");

        return group;
    }

    private static async Task<IResult> HandleAsync(string code,
        IShortLinkAppService shortLinkAppService,
        CancellationToken cancellationToken)
    {
        var result = await shortLinkAppService.GetOriginalUrlAsync(code, cancellationToken);

        return !result.Found ? Results.BadRequest(result) : Results.Ok(result);
    }
}