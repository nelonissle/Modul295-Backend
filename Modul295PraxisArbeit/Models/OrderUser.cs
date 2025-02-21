using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class OrderUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // ✅ Richtig als string

    [BsonElement("UserId")]
    public string UserId { get; set; } = ObjectId.GenerateNewId().ToString(); // ✅ Jetzt ein string

    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } = "user"; // ✅ Default-Wert "user"
    public string? TwoFactorCode { get; set; } // ✅ Added this

     // ✅ Two-Factor Authentication Fields
    [BsonElement("TwoFactorEnabled")]
    public bool TwoFactorEnabled { get; set; } = false; // ✅ 2FA enabled flag

    [BsonElement("TwoFactorSecret")]
    public string? TwoFactorSecret { get; set; } // ✅ Stores the user's TOTP secret

    [BsonElement("TwoFactorRecoveryCodes")]
    public List<string>? TwoFactorRecoveryCodes { get; set; } // ✅ Recovery codes for backup

    [BsonElement("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ✅ Timestamp when user was created
}
