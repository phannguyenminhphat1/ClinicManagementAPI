using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class AddPrescriptionDto
{
    [JsonPropertyName("prescription_id")]
    public int? PrescriptionId { get; set; }

    [JsonPropertyName("medical_record_detail_id")]
    public string? MedicalRecordDetailId { get; set; }

    [JsonPropertyName("prescription_details")]
    public List<AddPrescriptionDetailDto>? PrescriptionDetails { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}