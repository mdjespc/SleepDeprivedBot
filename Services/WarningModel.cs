using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class WarningModel{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public ulong UserId { get; set; }

    [BsonElement("username")]
    public string? Username { get ; set; }

    [BsonElement("guildId")]
    public ulong GuildId { get; set; }

    [BsonElement("guildName")]
    public string? GuildName { get; set; }

    [BsonElement("reason")]
    public string? Reason { get; set; }

    [BsonElement("created")]
    public required BsonDateTime Created { get; set; }

    [BsonElement("expires")]
    public BsonDateTime? Expires { get; set; }
}