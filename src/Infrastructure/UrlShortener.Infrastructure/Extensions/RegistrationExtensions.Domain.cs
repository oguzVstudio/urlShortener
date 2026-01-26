using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Services.CodeGenerators;
using UrlShortener.Application.Shared.Settings;
using UrlShortener.Infrastructure.Services.CodeGenerators;

namespace UrlShortener.Infrastructure.Extensions;

public static partial class RegistrationExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ShortLinkSettings>(configuration.GetSection(nameof(ShortLinkSettings)));
        services.AddSingleton<IUniqueCodeGenerator, UniqueCodeGenerator>();
        return services;
    }
}
