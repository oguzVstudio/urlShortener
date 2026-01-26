using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using UrlShortener.Domain.Shorten.Analytics;

namespace UrlShortener.Infrastructure.MongoDB.ModelConfigurations.Analytics;

public class ShortLinkAccessLogEntityConfiguration : IMongoEntityConfiguration
{
    private static bool _registered;
    public void Register()
    {
        if (_registered) return;
        _registered = true;
        
        BsonClassMap.RegisterClassMap<ShortLinkAccessLog>(cm =>
        {
            cm.AutoMap();
            
            cm.MapIdMember(x => x.Id)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard))
                .SetOrder(0);
            
            cm.MapMember(x => x.ShortLinkId)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

            cm.MapField(x => x.CreatedOnUtc)
                .SetSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
            
            cm.MapField(x => x.AccessedAt)
                .SetSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
        });
    }
}