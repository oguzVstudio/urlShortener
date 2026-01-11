using MassTransit;
using UrlShortener.Infrastructure.Masstransit.Consumers;

namespace UrlShortener.Infrastructure.Masstransit.RabbitMq.ConsumerEndpoints;

public static class UrlTrackingConsumerEndpoint
{
    public static void AddUrlTrackingConsumer(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("url_tracking_consumer_queue", e =>
        {
            e.Bind("url_tracking_consumer.input_exchange",
                x => x.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
            e.ConfigureConsumer<UrlTrackingConsumer>(context);

            e.UseMessageRetry(r =>
            {
                r.Interval(3, 5000);
            });
        });
    }
}
