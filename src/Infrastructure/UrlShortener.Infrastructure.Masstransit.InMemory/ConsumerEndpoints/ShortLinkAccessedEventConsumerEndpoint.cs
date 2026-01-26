using MassTransit;
using UrlShortener.Infrastructure.Masstransit.Consumers;

namespace UrlShortener.Infrastructure.Masstransit.InMemory.ConsumerEndpoints;

public static class ShortLinkAccessedEventConsumerEndpoint
{
    public static void AddUrlTrackingConsumer(
        this IInMemoryBusFactoryConfigurator cfg,
        IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("short_link_accessed_event_consumer_queue", e =>
        {
            e.Bind("short_link_accessed_event.input_exchange", MassTransit.Transports.Fabric.ExchangeType.FanOut);

            e.ConfigureConsumer<ShortLinkAccessedEventConsumer>(context);

            e.UseMessageRetry(r =>
            {
                r.Interval(3, 5000);
            });
        });
    }
}
