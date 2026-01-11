using MassTransit;
using UrlShortener.Infrastructure.Masstransit.Consumers;

namespace UrlShortener.Infrastrßucture.Masstransit.InMemory.ConsumerEndpoints;

public static class UrlTrackingConsumerEndpoint
{
    public static void AddUrlTrackingConsumer(
        this IInMemoryBusFactoryConfigurator cfg,
        IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("url_tracking_consumer_queue", e =>
        {
            e.Bind("url_tracking_consumer.input_exchange", MassTransit.Transports.Fabric.ExchangeType.FanOut);

            e.ConfigureConsumer<UrlTrackingConsumer>(context);

            e.UseMessageRetry(r =>
            {
                r.Interval(3, 5000);
            });
        });
    }
}
