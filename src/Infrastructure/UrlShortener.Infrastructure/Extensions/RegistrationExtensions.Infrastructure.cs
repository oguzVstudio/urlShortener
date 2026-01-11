using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UrlShortener.Infrastructure.Extensions;

public static class InfrastructureRegistrationExtensions
{
    /// <summary>
    /// Registers core infrastructure services (domain, application, cache, distributed lock)
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDomain(configuration)
            .AddApplication(configuration)
            .AddCache(configuration)
            .AddDistributedLock(configuration);
        
        return services;
    }
}