using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BazyDanychProjekt.Models;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.String)] // "userMateuszLogin_userKacperLogin..."
    public string Id { get; set; }

    [BsonElement("participants")]
    public List<string> Participants { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static string GenerateChatId(List<string> participants)
    {
        var sorted = participants.OrderBy(p => p).ToList();
        return string.Join("_", sorted);
    }
}
