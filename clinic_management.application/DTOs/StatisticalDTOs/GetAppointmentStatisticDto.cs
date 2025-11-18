using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

public class GetAppointmentStatisticDto
{
    [JsonPropertyName("doctor_id")]
    [FromQuery(Name = "doctor_id")]
    public string? DoctorId { get; set; }

    [JsonPropertyName("start_day")]
    [FromQuery(Name = "start_day")]
    public string? StartDay { get; set; }

    [JsonPropertyName("end_day")]
    [FromQuery(Name = "end_day")]
    public string? EndDay { get; set; }

    [JsonPropertyName("type")]
    [FromQuery(Name = "type")]
    public string? Type { get; set; }

    [JsonPropertyName("offset")]
    [FromQuery(Name = "offset")]
    public string? Offset { get; set; }
}