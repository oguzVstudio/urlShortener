using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Features.Shorten.Services.v1;
using UrlShortener.Application.Features.Shorten.Services.v1.Models;

namespace UrlShortener.Host.Features.Shorten.ShortenUrlCreation;

public static partial class ShortenUrlEndpoints
{
    public static RouteGroupBuilder MapShortenUrlEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/shorten", ShortenUrlAsync)
            .WithName("ShortenUrl")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> ShortenUrlAsync([FromBody] CreateShortenUrlRequest request,
        IShortenUrlAppService shortenUrlAppService,
        CancellationToken cancellationToken)
    {
        var result = await shortenUrlAppService.ShortenUrlAsync(new CreateShortUrlRequest(request.Url,
            request.IsExpiring,
            request.ExpiresAt), cancellationToken);

        return !result.Success ? Results.BadRequest() : Results.Ok(result);
    }
}