using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UpdateExaminingDto
{
    [JsonPropertyName("symptoms")]
    [Required(ErrorMessage = MedicalRecordMessages.SYMPTOMS_IS_REQUIRED)]
    public string? Symptoms { get; set; }

    [JsonPropertyName("diagnosis")]
    [Required(ErrorMessage = MedicalRecordMessages.DIAGNOSIS_IS_REQUIRED)]
    public string? Diagnosis { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}