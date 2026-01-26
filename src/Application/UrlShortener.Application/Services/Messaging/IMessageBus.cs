namespace UrlShortener.Application.Services.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, 
        IDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default) where T : class;
}