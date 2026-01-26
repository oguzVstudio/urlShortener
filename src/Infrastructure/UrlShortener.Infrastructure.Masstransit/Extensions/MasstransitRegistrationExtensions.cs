using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Services.Messaging;

namespace UrlShortener.Infrastructure.Masstransit.Extensions;

public static class MasstransitRegistrationExtensions
{
    public static IServiceCollection AddMasstransitBus(this IServiceCollection services)
    {
        services.AddTransient<IMessageBus, MasstransitBus>();
        return services;
    }
}