using UrlShortener.Application.Features.Shorten.Services.v1;
using UrlShortener.Application.Shared.Bus;
using UrlShortener.Domain.Shorten.ShortenUrls.Events;

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
        IShortenUrlAppService shortenUrlAppService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var shortenedUrl = await shortenUrlAppService.GetOriginalUrlAsync(code, cancellationToken);

        if (!shortenedUrl.Found)
        {
            return Results.NotFound();
        }

        var bus = httpContext.RequestServices.GetRequiredService<IBus>();
        await bus.PublishAsync(new UrlTrackingEvent
        {
            Code = code,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown",
            AccessedAt = DateTimeOffset.UtcNow
        }, cancellationToken: cancellationToken);

        return Results.Redirect(shortenedUrl.OriginalUrl!);
    }
}