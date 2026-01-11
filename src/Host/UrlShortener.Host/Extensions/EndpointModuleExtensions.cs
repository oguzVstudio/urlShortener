using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UrlShortener.Host.Features;

namespace UrlShortener.Host.Extensions;

public static class EndpointModuleExtensions
{
    public static IServiceCollection AddEndpointModules(this IServiceCollection services, params Assembly[] assemblies)
    {
        var moduleTypes = assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.IsAbstract && typeof(IEndpointModule).IsAssignableFrom(t))
            .Select(t => t.AsType())
            .ToList();

        foreach (var type in moduleTypes)
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IEndpointModule), type));

        return services;
    }

    public static WebApplication MapEndpointModules(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IEnumerable<IEndpointModule>>();
        foreach (var m in modules) m.Map(app);
        return app;
    }
}