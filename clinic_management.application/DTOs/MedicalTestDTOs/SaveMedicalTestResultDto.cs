using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class SaveMedicalTestResultDto
{
    [JsonPropertyName("medical_test_id")]
    [Required(ErrorMessage = MedicalTestMessages.MEDICAL_TEST_ID_IS_REQUIRED)]
    public string? MedicalTestId { get; set; }

    [JsonPropertyName("medical_test_result_id")]
    public int? MedicalTestResultId { get; set; }

    [JsonPropertyName("parameter")]
    [Required(ErrorMessage = MedicalTestMessages.PARAMETER_IS_REQUIRED)]
    public string Parameter { get; set; } = null!;

    [JsonPropertyName("value")]
    [Required(ErrorMessage = MedicalTestMessages.VALUE_IS_REQUIRED)]
    public string Value { get; set; } = null!;

    [JsonPropertyName("unit")]
    [Required(ErrorMessage = MedicalTestMessages.UNIT_IS_REQUIRED)]
    public string? Unit { get; set; }

    [JsonPropertyName("reference_range")]
    public string? ReferenceRange { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

}