using System.Text.Json.Serialization;

public class GetMessageDto
{
    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("conversation_id")]
    public int ConversationId { get; set; }

    [JsonPropertyName("sender_id")]
    public Guid SenderId { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("is_read")]
    public bool IsRead { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}