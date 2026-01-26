using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Features.Shorten.Services.v1;
using UrlShortener.Application.Features.Shorten.Services.v1.Models;

namespace UrlShortener.Host.Features.Shorten.ShortLinkCreation;

public static partial class ShortLinkEndpoints
{
    public static RouteGroupBuilder MapShortLinkEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/shorten", CreateShortLinkAsync)
            .WithName("CreateShortLink")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> CreateShortLinkAsync([FromBody] CreateLinkRequest request,
        IShortLinkAppService shortLinkAppService,
        CancellationToken cancellationToken)
    {
        var result = await shortLinkAppService.CreateShortLinkAsync(new CreateShortLinkRequest(request.Url,
            request.IsExpiring,
            request.ExpiresAt), cancellationToken);

        return !result.Success ? Results.BadRequest() : Results.Ok(result);
    }
}