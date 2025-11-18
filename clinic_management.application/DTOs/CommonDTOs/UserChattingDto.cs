
using System.Text.Json.Serialization;
using clinic_management.infrastructure.Models;

public class UserChattingDto : GetUsersDto
{
    [JsonPropertyName("conversation_id")]
    public int ConversationId { get; set; }


}