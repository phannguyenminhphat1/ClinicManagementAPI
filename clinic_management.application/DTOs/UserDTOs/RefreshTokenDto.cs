using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class RefreshTokenDto
{
    [JsonPropertyName("refresh_token")]
    [Required(ErrorMessage = AuthMessages.REFRESH_TOKEN_IS_REQUIRED)]
    public required string Token { get; set; }
}