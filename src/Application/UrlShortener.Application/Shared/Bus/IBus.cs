namespace UrlShortener.Application.Shared.Bus;

public interface IBus
{
    Task PublishAsync<T>(T message, 
        IDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default) where T : class;
}