using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Services.CodeGenerators;
using UrlShortener.Domain.Shorten.Settings;

namespace UrlShortener.Infrastructure.Extensions;

public static partial class RegistrationExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ShortenUrlSettings>(configuration.GetSection(nameof(ShortenUrlSettings)));
        services.AddSingleton<IUniqueCodeGenerator, UniqueCodeGenerator>();
        return services;
    }
}
