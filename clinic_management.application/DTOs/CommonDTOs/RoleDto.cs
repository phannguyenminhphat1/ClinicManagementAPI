using System.Text.Json.Serialization;

public class RoleDto
{
    [JsonPropertyName("role_id")]
    public int RoleId { get; set; }

    [JsonPropertyName("role_name")]
    public string RoleName { get; set; } = string.Empty;
}