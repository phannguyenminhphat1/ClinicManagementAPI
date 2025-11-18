using System.Text.Json.Serialization;
public class GetAppointmentDto
{
    [JsonPropertyName("appointment_id")]
    public int AppointmentId { get; set; }

    [JsonPropertyName("scheduled_date")]
    public DateTime ScheduledDate { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("patient")]
    public PatientDto Patient { get; set; } = new PatientDto();

    [JsonPropertyName("doctor")]
    public DoctorDto Doctor { get; set; } = new DoctorDto();

    // [JsonPropertyName("status")]
    // public StatusDto Status { get; set; } = new StatusDto();
    [JsonPropertyName("status_id")]
    public int? StatusId { get; set; }

    [JsonPropertyName("status_name")]
    public string? StatusName { get; set; }
}




