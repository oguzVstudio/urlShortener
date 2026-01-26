using MassTransit;
using UrlShortener.Infrastructure.Masstransit.Consumers;

namespace UrlShortener.Infrastructure.Masstransit.RabbitMq.ConsumerEndpoints;

public static class ShortLinkAccessedEventConsumerEndpoint
{
    public static void AddShortLinkAccessedEventConsumer(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("short_link_accessed_event_consumer_queue", e =>
        {
            e.Bind("short_link_accessed_event.input_exchange",
                x => x.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
            e.ConfigureConsumer<ShortLinkAccessedEventConsumer>(context);

            e.UseMessageRetry(r =>
            {
                r.Interval(3, 5000);
            });
        });
    }
}
