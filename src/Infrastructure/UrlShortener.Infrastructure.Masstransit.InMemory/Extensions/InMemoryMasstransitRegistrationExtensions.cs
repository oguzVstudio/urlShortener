using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Infrastructure.Masstransit.Consumers;
using UrlShortener.Infrastructure.Masstransit.Extensions;
using UrlShortener.Infrastructure.Masstransit.InMemory.ConsumerEndpoints;
using UrlShortener.Infrastructure.Masstransit.InMemory.Publishers;

namespace UrlShortener.Infrastructure.Masstransit.InMemory.Extensions;

public static class InMemoryMasstransitRegistrationExtensions
{
    public static IServiceCollection AddInMemoryMasstransit(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationContext, IInMemoryBusFactoryConfigurator> configureReceiveEndpoints = null,
        Action<IBusRegistrationConfigurator>? configureBusRegistration = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        if (configureReceiveEndpoints == null) throw new ArgumentNullException(nameof(configureReceiveEndpoints));
        if (configureBusRegistration == null) throw new ArgumentNullException(nameof(configureBusRegistration));
        services.AddMassTransit(ConfiguratorAction);

        void ConfiguratorAction(IBusRegistrationConfigurator busRegistrationConfigurator)
        {
            configureBusRegistration?.Invoke(busRegistrationConfigurator);
            busRegistrationConfigurator.AddDelayedMessageScheduler();
            busRegistrationConfigurator.SetEndpointNameFormatter(
                new SnakeCaseEndpointNameFormatter(false));

            busRegistrationConfigurator.UsingInMemory((context, cfg) =>
            {
                cfg.UseDelayedMessageScheduler();
                cfg.UseMessageRetry(r => AddRetryConfiguration(r));

                configureReceiveEndpoints?.Invoke(context, cfg);
            });
        }
        
        return services;
    }

    public static IServiceCollection AddInMemoryMasstransit(this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddMasstransitBus()
            .AddInMemoryMasstransit(
            configuration,
            (context, cfg) =>
            {
                cfg.AddUrlTrackingConsumer(context);

                cfg.AddShortLinkAccessedEventPublisher();
            },
            configureBusRegistration: x =>
            {
                x.AddConsumer<ShortLinkAccessedEventConsumer>();
            });
        
        return services;
    }
    
    private static IRetryConfigurator AddRetryConfiguration(IRetryConfigurator retryConfigurator)
    {
        retryConfigurator
            .Exponential(3, TimeSpan.FromMilliseconds(200), TimeSpan.FromMinutes(120), TimeSpan.FromMilliseconds(200));

        return retryConfigurator;
    }
}