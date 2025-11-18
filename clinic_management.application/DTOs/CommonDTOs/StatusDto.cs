using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

public class StatusDto
{
    [JsonPropertyName("status_id")]
    [FromQuery(Name = "status_id")]

    public int StatusId { get; set; }

    [JsonPropertyName("status_name")]
    [FromQuery(Name = "status_name")]
    public string? StatusName { get; set; }
}