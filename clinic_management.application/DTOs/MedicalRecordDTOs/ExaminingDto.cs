using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class ExaminingDto
{
    [JsonPropertyName("symptoms")]
    public string? Symptoms { get; set; }

    [JsonPropertyName("diagnosis")]
    public string? Diagnosis { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("requires_test")]
    public bool? RequiresTest { get; set; }

    [JsonPropertyName("service_ids")]
    public List<string>? ServiceIds { get; set; }
}