using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Features.Shorten.Services.v1;

namespace UrlShortener.Infrastructure.Extensions;

public static partial class RegistrationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IShortLinkAppService, ShortLinkAppService>();
        return services;
    }
}
