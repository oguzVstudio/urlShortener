namespace UrlShortener.Application.Shared.Cache;

public abstract class DistributedCacheKey
{
    protected virtual string Prefix => "App";

    private string Separator => ":";

    public string Key { get; private set; }

    protected DistributedCacheKey(string key)
    {
        Key = Prefix + Separator + key;
    }
}