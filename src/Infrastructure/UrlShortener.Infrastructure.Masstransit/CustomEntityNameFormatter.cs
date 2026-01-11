using MassTransit;

namespace UrlShortener.Infrastructure.Masstransit;

public class CustomEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        return typeof(T).Name;
    }
}