using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Shared.Bus;

namespace UrlShortener.Infrastructure.Masstransit.Extensions;

public static class MasstransitRegistrationExtensions
{
    public static IServiceCollection AddMasstransitBus(this IServiceCollection services)
    {
        services.AddTransient<IBus, MasstransitBus>();
        return services;
    }
}