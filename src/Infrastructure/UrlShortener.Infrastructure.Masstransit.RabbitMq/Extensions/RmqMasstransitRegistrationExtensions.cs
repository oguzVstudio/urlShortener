using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Infrastructure.Masstransit.Consumers;
using UrlShortener.Infrastructure.Masstransit.Extensions;
using UrlShortener.Infrastructure.Masstransit.RabbitMq.ConsumerEndpoints;
using UrlShortener.Infrastructure.Masstransit.RabbitMq.Publishers;

namespace UrlShortener.Infrastructure.Masstransit.RabbitMq.Extensions;

public static class RmqMasstransitRegistrationExtensions
{
    public static IServiceCollection AddRabbitMqMasstransit(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureReceiveEndpoints = null,
        Action<IBusRegistrationConfigurator> configureBusRegistration = null)
    {
        services.AddMassTransit(ConfiguratorAction);

        void ConfiguratorAction(IBusRegistrationConfigurator busRegistrationConfigurator)
        {
            configureBusRegistration?.Invoke(busRegistrationConfigurator);
            busRegistrationConfigurator.AddDelayedMessageScheduler();
            busRegistrationConfigurator.SetEndpointNameFormatter(
                new SnakeCaseEndpointNameFormatter(false));

            busRegistrationConfigurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.PublishTopology.BrokerTopologyOptions = PublishBrokerTopologyOptions.FlattenHierarchy;

                IConfigurationSection sectionData = configuration.GetSection("RabbitMqOptions");
                var rabbitMqOptions = new RabbitMqOptions();
                sectionData.Bind(rabbitMqOptions);

                cfg.Host(
                    rabbitMqOptions.Host,
                    rabbitMqOptions.Port,
                    rabbitMqOptions.VirtualHost,
                    hostConfigurator =>
                    {
                        hostConfigurator.Username(rabbitMqOptions.UserName);
                        hostConfigurator.Password(rabbitMqOptions.Password);
                    }
                );
                cfg.UseDelayedMessageScheduler();

                cfg.UseMessageRetry(r => AddRetryConfiguration(r));

                cfg.MessageTopology.SetEntityNameFormatter(new CustomEntityNameFormatter());


                configureReceiveEndpoints?.Invoke(context, cfg);
            });
        }

        return services;
    }

    public static IServiceCollection AddRabbitMqMasstransit(this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddMasstransitBus()
            .AddRabbitMqMasstransit(
                configuration,
                (context, cfg) =>
                {
                    cfg.AddUrlTrackingConsumer(context);

                    cfg.AddShortenedUrlTrackPublisher();
                },
                configureBusRegistration: x => { x.AddConsumer<UrlTrackingConsumer>(); });

        return services;
    }

    private static IRetryConfigurator AddRetryConfiguration(IRetryConfigurator retryConfigurator)
    {
        retryConfigurator
            .Exponential(3, TimeSpan.FromMilliseconds(200), TimeSpan.FromMinutes(120), TimeSpan.FromMilliseconds(200));

        return retryConfigurator;
    }
}