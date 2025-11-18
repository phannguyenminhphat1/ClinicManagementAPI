using System.Text.Json.Serialization;

namespace clinic_management_api.Models { }

public class GetUsersAdminDto
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("fullname")]
    public string Fullname { get; set; } = null!;

    [JsonPropertyName("gender")]
    public byte Gender { get; set; }

    [JsonPropertyName("birth_date")]
    public DateOnly? BirthDate { get; set; }

    public string? Phone { get; set; }

    [JsonPropertyName("role_name")]
    public string? RoleName { get; set; }

    [JsonPropertyName("specialty_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SpecialtyName { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

}
