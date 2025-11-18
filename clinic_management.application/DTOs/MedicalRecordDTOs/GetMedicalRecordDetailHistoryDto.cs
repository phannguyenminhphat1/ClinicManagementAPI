using System.Text.Json.Serialization;
using clinic_management.infrastructure.Models;

public class GetMedicalRecordDetailHistoryDto
{
    [JsonPropertyName("medical_record_detail_id")]
    public int MedicalRecordDetailId { get; set; }

    [JsonPropertyName("appointment")]
    public GetAppointmentDto? Appointment { get; set; }

    [JsonPropertyName("symptoms")]
    public string Symptoms { get; set; } = null!;

    [JsonPropertyName("diagnosis")]
    public string? Diagnosis { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("requires_tests")]
    public bool RequiresTest { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

}