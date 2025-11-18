using System.Text.Json.Serialization;

public class GetMeDto
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("fullname")]
    public string Fullname { get; set; } = null!;

    [JsonPropertyName("gender")]
    public byte? Gender { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("birth_date")]
    public DateOnly BirthDate { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("role_name")]
    public string? RoleName { get; set; }

    [JsonPropertyName("specialty_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SpecialtyName { get; set; }

}
