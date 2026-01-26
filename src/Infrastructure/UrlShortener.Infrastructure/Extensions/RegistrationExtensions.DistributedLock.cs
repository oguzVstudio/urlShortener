using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Services.Caching;
using UrlShortener.Infrastructure.Cache;

namespace UrlShortener.Infrastructure.Extensions;

public static partial class RegistrationExtensions
{
    public static IServiceCollection AddDistributedLock(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDistributedLock, DistributedLock>();
        return services;
    }
}