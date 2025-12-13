using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Config;

public class EntityAccount
{
    [JsonPropertyName("id")] public int? Id { get; set; }

    [JsonPropertyName("name")] public string? Name { get; init; }

    [JsonPropertyName("account")] public string? Account { get; set; }

    [JsonPropertyName("userId")] public string? UserId { get; set; }

    [JsonPropertyName("type")] public string? Type { get; init; }

    [JsonPropertyName("password")] public string? Password { get; set; }

    public bool Equals(EntityAccount other)
    {
        return Name == other.Name && Account == other.Account && Type == other.Type && Password == other.Password;
    }

    public new string ToString()
    {
        return $"Name: {Name}, Account: {Account}, Type: {Type}, Password: {Password}";
    }
}