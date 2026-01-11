using MassTransit;
using UrlShortener.Domain.Shorten.ShortenUrls.Events;

namespace UrlShortener.Infrastructure.Masstransit.InMemory.Publishers;

public static class ShortenedUrlTrackPublisher
{
    public static void AddShortenedUrlTrackPublisher(this IInMemoryBusFactoryConfigurator cfg)
    {
        cfg.Message<UrlTrackingEvent>(x =>
        {
            x.SetEntityName("url_tracking_event.input_exchange");
        });

        cfg.Publish<UrlTrackingEvent>(x =>
        {
            x.ExchangeType = MassTransit.Transports.Fabric.ExchangeType.FanOut;
        });
    }
}
