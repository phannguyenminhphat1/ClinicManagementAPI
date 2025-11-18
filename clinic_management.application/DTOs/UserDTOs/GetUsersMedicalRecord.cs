using System.Text.Json.Serialization;

public class GetUsersMedicalRecordDto
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("fullname")]
    public string? Fullname { get; set; }

    [JsonPropertyName("gender")]
    public byte Gender { get; set; }

    [JsonPropertyName("birth_date")]
    public DateOnly? BirthDate { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("role_name")]
    public string? RoleName { get; set; }

    [JsonPropertyName("medical_record_id")]
    public int MedicalRecordId { get; set; }

    [JsonPropertyName("medical_records_detail_id")]
    public List<int>? MedicalRecordDetailId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
}