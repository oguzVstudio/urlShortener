using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using UrlShortener.Domain.Shorten.ShortLinks;

namespace UrlShortener.Infrastructure.MongoDB.ModelConfigurations.Shorten;

public class ShortLinkEntityConfiguration : IMongoEntityConfiguration
{
    private static bool _registered;
    
    public void Register()
    {
        if (_registered) return;
        _registered = true;
        
        BsonClassMap.RegisterClassMap<ShortLink>(cm =>
        {
            cm.AutoMap();
            
            cm.MapIdMember(x => x.Id)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard))
                .SetOrder(0);

            cm.MapField(x => x.CreatedOnUtc)
                .SetSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
            
            cm.MapMember(x => x.ExpiresAt)
                .SetSerializer(new NullableSerializer<DateTimeOffset>(
                    new DateTimeOffsetSerializer(BsonType.DateTime)
                ));
        });
    }
}