using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Infrastructure.Settings;

namespace UrlShortener.Infrastructure.Extensions;

public static partial class RegistrationExtensions
{
    public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            var redisOptionSection = configuration.GetSection("RedisOptions");
            var redisOptions = redisOptionSection.Get<RedisOptions>();
            var connectionString = $"{redisOptions!.Host}:{redisOptions.Port},password={redisOptions.Password},ssl=false";
            options.Configuration = connectionString;
        });

        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024 * 10;
            options.MaximumKeyLength = 512;

            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),                
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            };
        });

        return services;
    }
}
