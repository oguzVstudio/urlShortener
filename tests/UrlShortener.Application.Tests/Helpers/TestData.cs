namespace UrlShortener.Application.Tests.Helpers;

/// <summary>
/// Common test data for use across test classes
/// </summary>
public static class TestData
{
    public static class Urls
    {
        public const string ValidLongUrl = "https://www.example.com/very/long/url/path";
        public const string GoogleUrl = "https://www.google.com";
        public const string MicrosoftUrl = "https://www.microsoft.com/products";
        public const string GitHubUrl = "https://github.com/user/repo/issues/123";
    }

    public static class Codes
    {
        public const string ValidCode = "abc123";
        public const string AlternativeCode = "xyz789";
        public const string NonExistentCode = "notfound";
    }

    public static class IpAddresses
    {
        public const string LocalHost = "127.0.0.1";
        public const string PrivateNetwork = "192.168.1.1";
        public const string ExternalIp = "203.0.113.42";
    }

    public static class UserAgents
    {
        public const string Chrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
        public const string Firefox = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0";
        public const string Safari = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15";
    }
}
