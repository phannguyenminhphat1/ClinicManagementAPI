using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class SendMessageDto
{
    [JsonPropertyName("conversation_id")]
    [Required(ErrorMessage = ChattingMessages.CONVERSATION_ID_IS_REQUIRED)]
    public string? ConversationId { get; set; }

    [JsonPropertyName("receiver_id")]
    [Required(ErrorMessage = ChattingMessages.RECEIVER_ID_IS_REQUIRED)]
    public string? ReceiverId { get; set; }

    [JsonPropertyName("content")]
    [Required(ErrorMessage = ChattingMessages.CONTENT_IS_REQUIRED)]
    public string? Content { get; set; }


}