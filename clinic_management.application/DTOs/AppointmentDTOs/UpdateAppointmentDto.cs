using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UpdateAppointmentDto
{
    [JsonPropertyName("date")]
    public string? Date { get; set; } // Chỉ ngày

    [JsonPropertyName("time")]
    public string? Time { get; set; } = string.Empty; // "08:30" format

    [JsonPropertyName("status_id")]
    public string? StatusId { get; set; }

    [JsonPropertyName("doctor_id")]
    public string? DoctorId { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}
