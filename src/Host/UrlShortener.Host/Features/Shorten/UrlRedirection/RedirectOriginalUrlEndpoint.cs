using UrlShortener.Application.Features.Shorten.Services.v1;
using UrlShortener.Application.Services.Messaging;
using UrlShortener.Domain.Shared.Events;

namespace UrlShortener.Host.Features.Shorten.UrlRedirection;

public static class RedirectOriginalUrlEndpoint
{
    public static IEndpointRouteBuilder MapRedirectOriginalUrlEndpoint(this IEndpointRouteBuilder group)
    {
        group.MapGet("{code}", HandleAsync)
            .WithName("Redirect Original Url")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireRateLimiting("fixed");

        return group;
    }

    private static async Task<IResult> HandleAsync(string code,
        IShortLinkAppService shortLinkAppService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await shortLinkAppService.GetOriginalUrlAsync(code, cancellationToken);

        if (!result.Found)
        {
            return Results.NotFound();
        }

        var bus = httpContext.RequestServices.GetRequiredService<IMessageBus>();
        await bus.PublishAsync(new ShortLinkAccessedEvent
        {
            Code = code,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown",
            AccessedAt = DateTimeOffset.UtcNow
        }, cancellationToken: cancellationToken);

        return Results.Redirect(result.OriginalUrl!);
    }
}