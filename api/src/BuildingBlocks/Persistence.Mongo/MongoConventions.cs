using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace EmailPlatform.BuildingBlocks.Persistence.Mongo;

public static class MongoConventions
{
    private static readonly object _lock = new();
    private static bool _registered;

    public static void RegisterOnce()
    {
        if (_registered) return;
        lock (_lock)
        {
            if (_registered) return;

            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            };
            ConventionRegistry.Register("EmailPlatform", pack, _ => true);

            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            _registered = true;
        }
    }
}
