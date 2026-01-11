using UrlShortener.Application.Shared.Cache;

namespace UrlShortener.Application.Features.Shared.Cache;

public class ShortenDistributedCacheKey(string key) : DistributedCacheKey(key)
{
    protected override string Prefix => "Shorten";
}