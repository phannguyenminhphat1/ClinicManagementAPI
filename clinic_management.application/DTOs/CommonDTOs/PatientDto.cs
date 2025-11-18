
using System.Text.Json.Serialization;

public class PatientDto
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("fullname")]
    public string? Fullname { get; set; }

    [JsonPropertyName("gender")]
    public byte Gender { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("birth_date")]
    public DateOnly? BirthDate { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }
}