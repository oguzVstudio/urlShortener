namespace UrlShortener.Host.Features;

public interface IEndpointModule
{
    void Map(IEndpointRouteBuilder app);
}