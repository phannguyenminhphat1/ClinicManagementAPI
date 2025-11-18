using System.Text.Json.Serialization;

public class CreateBillingDto
{
    [JsonPropertyName("appointment_id")]
    public string? AppointmentId { get; set; }
}