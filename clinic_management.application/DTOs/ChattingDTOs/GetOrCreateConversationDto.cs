using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class GetOrCreateConversationDto
{
    [JsonPropertyName("user2_id")]
    [Required(ErrorMessage = ChattingMessages.USER_ID_IS_REQUIRED)]
    public string? User2Id { get; set; }

}