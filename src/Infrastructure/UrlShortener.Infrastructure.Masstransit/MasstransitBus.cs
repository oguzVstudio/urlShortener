using MassTransit;
using UrlShortener.Domain.Extensions;
using IBus = UrlShortener.Application.Shared.Bus.IBus;

namespace UrlShortener.Infrastructure.Masstransit;

public class MasstransitBus(IPublishEndpoint publishEndpoint) : IBus
{
    public async Task PublishAsync<T>(T message,
        IDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var meta = headers ?? new Dictionary<string, object?>();

        meta = GetMetadata(message, meta);

        await publishEndpoint.Publish(
            message,
            ctx =>
            {
                foreach (var header in meta)
                {
                    ctx.Headers.Set(header.Key, header.Value);
                }
            },
            cancellationToken
        );
    }
    
    private static IDictionary<string, object?> GetMetadata<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers) where TMessage : class
    {
        var meta = headers ?? new Dictionary<string, object?>();
        meta.Add("name", message.GetType().Name);
        meta.Add("type", message.GetType().Name);
        meta.Add("created", DatetimeExtensions.ToUnixTimeSecond(DateTime.Now));

        return meta;
    }
}