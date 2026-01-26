using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten;
using UrlShortener.Infrastructure.MongoDB.Context;
using UrlShortener.Infrastructure.MongoDB.ModelConfigurations;
using UrlShortener.Infrastructure.MongoDB.Stores;

namespace UrlShortener.Infrastructure.MongoDB.Extensions;

public static class MongoDbRegistrationExtensions
{
    /// <summary>
    /// Register complete MongoDB persistence layer for URL Shortener domain
    /// </summary>
    public static IServiceCollection AddShortenMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddShortenMongoDbContext(configuration)
            .AddShortenMongoDbStores();
    }

    public static IServiceCollection AddShortenMongoDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<MongoDbOptions>().BindConfiguration(nameof(MongoDbOptions));
        services.AddSingleton(x => x.GetRequiredService<IOptions<MongoDbOptions>>().Value);
        services.AddScoped(typeof(ShortenMongoDbContext));
        services.AddScoped<IShortenBaseDbContext>(provider => provider.GetRequiredService<ShortenMongoDbContext>());
        
        RegisterModelConfigurations();

        return services;
    }
    
    public static IServiceCollection AddShortenMongoDbStores(this IServiceCollection services)
    {
        services.AddScoped<IShortLinkStore, ShortLinkMongoStore>();
        services.AddScoped<IShortLinkAccessLogStore, ShortLinkAccessLogMongoStore>();

        return services;
    }

    private static void RegisterModelConfigurations()
    {
        var assembliesToScan = new[] { Assembly.GetExecutingAssembly() };

        var configTypes = assembliesToScan
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.IsAbstract && typeof(IMongoEntityConfiguration).IsAssignableFrom(t))
            .Select(t => t.AsType());

        foreach (var type in configTypes)
        {
            var instance = (IMongoEntityConfiguration)Activator.CreateInstance(type)!;
            instance.Register();
        }
    }
}