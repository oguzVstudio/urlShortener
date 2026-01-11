namespace UrlShortener.Infrastructure.Settings;

public class RedisOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string Password { get; set; } = default!;
}
