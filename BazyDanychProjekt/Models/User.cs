using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BazyDanychProjekt.Models;
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("login")]
    public string Login { get; set; }

    [BsonElement("password")]
    public string Password { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("firstName")]
    public string FirstName { get; set; }

    [BsonElement("lastName")]
    public string LastName { get; set; }
}
