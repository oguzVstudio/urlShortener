using MassTransit;
using UrlShortener.Domain.Shared.Events;

namespace UrlShortener.Infrastructure.Masstransit.InMemory.Publishers;

public static class ShortLinkAccessedEventPublisher
{
    public static void AddShortLinkAccessedEventPublisher(this IInMemoryBusFactoryConfigurator cfg)
    {
        cfg.Message<ShortLinkAccessedEvent>(x =>
        {
            x.SetEntityName("short_link_accessed_event.input_exchange");
        });

        cfg.Publish<ShortLinkAccessedEvent>(x =>
        {
            x.ExchangeType = MassTransit.Transports.Fabric.ExchangeType.FanOut;
        });
    }
}
