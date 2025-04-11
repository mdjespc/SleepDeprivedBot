using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class GuildSettingsModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("guildId")]
    public ulong? GuildId { get; set; }

    [BsonElement("prefix")]
    public string Prefix { get; set; } = "!";

    [BsonElement("language")]
    public string Language { get; set; } = "en";

    [BsonElement("welcomeChannel")]
    public string? WelcomeChannel { get; set;}

    [BsonElement("welcomeMessage")]
    public string? WelcomeMessage { get; set;}

    [BsonElement("modlog")]
    public string? Modlog { get; set;}
}
