using System.Text.Json.Serialization;

public class GetConversationDto
{
    [JsonPropertyName("conversation_id")]
    public int ConversationId { get; set; }

    [JsonPropertyName("user1_id")]
    public Guid User1Id { get; set; }

    [JsonPropertyName("user2_id")]
    public Guid User2Id { get; set; }

    // [JsonPropertyName("messages")]
    // public List<GetMessageDto> Messages { get; set; } = new();

    [JsonPropertyName("last_message_id")]
    public int? LastMessageId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }



}