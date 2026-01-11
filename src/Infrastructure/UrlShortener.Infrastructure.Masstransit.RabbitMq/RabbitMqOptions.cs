namespace UrlShortener.Infrastructure.Masstransit.RabbitMq;

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 5672;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string VirtualHost { get; set; } = "/";
    public string ConnectionString => $"amqp://{UserName}:{Password}@{Host}:{Port}/";
}