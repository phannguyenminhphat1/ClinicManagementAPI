using System.Text.Json.Serialization;

public class AppointmentStatusHistoryDto
{
    [JsonPropertyName("history_id")]
    public int HistoryId { get; set; }

    [JsonPropertyName("appointment_id")]
    public int AppointmentId { get; set; }

    [JsonPropertyName("status_id")]
    public int StatusId { get; set; }

    [JsonPropertyName("status_name")]
    public string? StatusName { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
}