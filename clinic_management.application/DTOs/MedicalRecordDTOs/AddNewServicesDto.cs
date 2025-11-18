using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class AddNewServicesDto
{
    [JsonPropertyName("medical_record_detail_id")]
    [Required(ErrorMessage = MedicalRecordMessages.MEDICAL_RECORD_DETAIL_ID_IS_REQUIRED)]
    public string? MedicalRecordDetailId { get; set; }

    [JsonPropertyName("service_ids")]
    public List<string>? ServiceIds { get; set; }
}