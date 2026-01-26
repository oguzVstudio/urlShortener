using MassTransit;
using UrlShortener.Domain.Shared.Events;

namespace UrlShortener.Infrastructure.Masstransit.RabbitMq.Publishers;

public static class ShortLinkAccessedEventPublisher
{
    public static void AddShortLinkAccessedEventPublisher(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<ShortLinkAccessedEvent>(x =>
        {
            x.SetEntityName("short_link_accessed_event.input_exchange");
        });

        cfg.Publish<ShortLinkAccessedEvent>(
            e => 
                e.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
    }
}
