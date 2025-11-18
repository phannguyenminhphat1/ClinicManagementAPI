using System.Text.Json.Serialization;

public class GetMedicalRecordDto
{
    [JsonPropertyName("medical_record_id")]
    public int MedicalRecordId { get; set; }

    [JsonPropertyName("patient")]
    public PatientDto Patient { get; set; } = new PatientDto();

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }


}