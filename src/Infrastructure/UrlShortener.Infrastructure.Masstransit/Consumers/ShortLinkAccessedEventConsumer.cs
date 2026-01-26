using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Features.Analytics.Services.v1;
using UrlShortener.Application.Features.Analytics.Services.v1.Models;
using UrlShortener.Domain.Shared.Events;

namespace UrlShortener.Infrastructure.Masstransit.Consumers;

public class ShortLinkAccessedEventConsumer : IConsumer<ShortLinkAccessedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ShortLinkAccessedEventConsumer> _logger;

    public ShortLinkAccessedEventConsumer(IServiceScopeFactory serviceScopeFactory, ILogger<ShortLinkAccessedEventConsumer> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ShortLinkAccessedEvent> context)
    {
        try
        {
            var message = context.Message;
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IShortLinkAnalyticsAppService>();

            await service.CreateShortLinkAccessLogAsync(new CreateShortLinkAccessLogRequest(
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
