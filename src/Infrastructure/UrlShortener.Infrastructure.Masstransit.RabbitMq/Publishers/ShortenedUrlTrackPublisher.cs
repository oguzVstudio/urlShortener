using MassTransit;
using UrlShortener.Domain.Shorten.ShortenUrls.Events;

namespace UrlShortener.Infrastructure.Masstransit.RabbitMq.Publishers;

public static class ShortenedUrlTrackPublisher
{
    public static void AddShortenedUrlTrackPublisher(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<UrlTrackingEvent>(x =>
        {
            x.SetEntityName("url_tracking_event.input_exchange");
        });

        cfg.Publish<UrlTrackingEvent>(
            e => 
                e.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
    }
}
