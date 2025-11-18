using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class MakeAppointmentWithDoctorDto
{
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("fullname")]
    public string Fullname { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public byte? Gender { get; set; }

    [JsonPropertyName("birth_date")]
    public string? BirthDate { get; set; }

    [JsonPropertyName("doctor_id")]
    public string? DoctorId { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; } // Chỉ ngày

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty; // "08:30" format

    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;

    [JsonPropertyName("is_existing_patient")]
    public bool IsExistingPatient { get; set; }


}