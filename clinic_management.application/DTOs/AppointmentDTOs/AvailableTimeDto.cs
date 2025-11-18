using System.Text.Json.Serialization;

public class AvailableTimeDto
{
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty; // ex: "08:00"

    [JsonPropertyName("is_available")]
    public bool IsAvailable { get; set; } // true if at least one doctor free
}
