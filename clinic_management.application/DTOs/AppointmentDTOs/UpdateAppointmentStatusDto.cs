using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UpdateAppointmentStatusDto
{
    [JsonPropertyName("status_id")]
    public string? StatusId { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}
