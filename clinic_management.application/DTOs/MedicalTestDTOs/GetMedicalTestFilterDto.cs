using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
public class GetMedicalTestFilterDto
{
    [JsonPropertyName("status")]
    [FromQuery(Name = "status")]
    public string? StatusId { get; set; }

    [JsonPropertyName("date")]
    [FromQuery(Name = "date")]
    public string? Date { get; set; }

    [JsonPropertyName("keyword")]
    [FromQuery(Name = "keyword")]
    public string? Keyword { get; set; }

}




