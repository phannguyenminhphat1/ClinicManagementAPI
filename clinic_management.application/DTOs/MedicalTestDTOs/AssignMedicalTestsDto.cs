using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class AssignMedicalTestsDto
{
    [JsonPropertyName("medical_record_detail_id")]
    [Required(ErrorMessage = MedicalRecordMessages.MEDICAL_RECORD_ID_IS_REQUIRED)]
    public string MedicalRecordDetailId { get; set; } = string.Empty;

    [JsonPropertyName("service_ids")]
    [Required(ErrorMessage = MedicalRecordMessages.SERVICE_ID_IS_REQUIRED)]
    public List<string> ServiceIds { get; set; } = new();
}
