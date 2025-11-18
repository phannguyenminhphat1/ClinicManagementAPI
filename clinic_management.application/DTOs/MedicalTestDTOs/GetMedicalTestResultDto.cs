using System.Text.Json.Serialization;

public class GetMedicalTestResultDto
{
    [JsonPropertyName("medical_test_result_id")]
    public int MedicalTestResultId { get; set; }

    [JsonPropertyName("parameter")]
    public string? Parameter { get; set; } = null!;

    [JsonPropertyName("value")]
    public string? Value { get; set; } = null!;

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("reference_range")]
    public string? ReferenceRange { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

}