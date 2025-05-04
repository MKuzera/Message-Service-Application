using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BazyDanychProjekt.Models;

public class Message
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("chatId")]
    public string ChatId { get; set; }

    [BsonElement("senderLogin")]
    public string SenderLogin { get; set; }

    [BsonElement("message")]
    public string Text { get; set; }

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    string GenerateChatId(List<string> participants)
    {
        var sorted = participants.OrderBy(p => p).ToList();
        return string.Join("_", sorted);
    }
}
