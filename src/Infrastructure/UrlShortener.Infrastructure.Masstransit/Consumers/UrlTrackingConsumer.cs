using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Features.Shorten.Services.v1;
using UrlShortener.Application.Features.Shorten.Services.v1.Models;
using UrlShortener.Domain.Shorten.ShortenUrls.Events;

namespace UrlShortener.Infrastructure.Masstransit.Consumers;

public class UrlTrackingConsumer : IConsumer<UrlTrackingEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<UrlTrackingConsumer> _logger;

    public UrlTrackingConsumer(IServiceScopeFactory serviceScopeFactory, ILogger<UrlTrackingConsumer> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UrlTrackingEvent> context)
    {
        try
        {
            var message = context.Message;
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IShortenUrlAppService>();

            await service.TrackUrlAccessAsync(new CreateShortUrlTrackRequest(
                Code: message.Code,
                UserAgent: message.UserAgent,
                IpAddress: message.IpAddress,
                AccessedAt: message.AccessedAt
            ), context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UrlTrackingEvent for Code: {Code}", context.Message.Code);
            throw;
        }
    }
}
