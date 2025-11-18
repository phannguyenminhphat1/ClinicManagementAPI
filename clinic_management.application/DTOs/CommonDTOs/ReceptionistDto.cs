
using System.Text.Json.Serialization;

public class ReceptionistDto
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("fullname")]
    public string? Fullname { get; set; }

    [JsonPropertyName("gender")]
    public byte Gender { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

}