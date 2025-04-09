using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserModel
{
    #pragma warning disable CS8618
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userId")]
    public ulong? UserId { get; set; }

    [BsonElement("discriminator")]
    public string? Discriminator { get; set; }

    [BsonElement("level")]
    public int Level { get; set; } = 1;

    [BsonElement("experience")]
    public int Experience { get; set;} = 0;

    [BsonElement("currency")]
    public int Currency {get; set;} = 0;
}
