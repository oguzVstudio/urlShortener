using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Infrastructure.MongoDB.ModelConfigurations.Shorten;

public class ShortUrlTrackEntityConfiguration : IMongoEntityConfiguration
{
    private static bool _registered;
    public void Register()
    {
        if (_registered) return;
        _registered = true;
        
        BsonClassMap.RegisterClassMap<ShortenUrlTrack>(cm =>
        {
            cm.AutoMap();
            
            cm.MapIdMember(x => x.Id)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard))
                .SetOrder(0);
            
            cm.MapMember(x => x.ShortenUrlId)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

            cm.MapField(x => x.CreatedOnUtc)
                .SetSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
            
            cm.MapField(x => x.AccessedAt)
                .SetSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
        });
    }
}