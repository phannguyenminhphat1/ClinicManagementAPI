using System.Text.Json.Serialization;

public class GetMedicalTestDto
{
    [JsonPropertyName("medical_test_id")]
    public int MedicalTestId { get; set; }

    [JsonPropertyName("patient_name")]
    public string? Fullname { get; set; }

    [JsonPropertyName("service")]
    public GetServiceDto? Service { get; set; }

    [JsonPropertyName("status_id")]
    public int? StatusId { get; set; }

    [JsonPropertyName("status_name")]
    public string? StatusName { get; set; }

    [JsonPropertyName("list_medical_test_result")]
    public List<GetMedicalTestResultDto>? MedicalTestResults { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

}