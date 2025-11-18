using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public class GetAvailableTimeSlotsDoctorDto
{
    [JsonPropertyName("date")]
    [Required(ErrorMessage = AppointmentMessages.DATE_IS_REQUIRED)]
    public string? Date { get; set; }

    [JsonPropertyName("doctor_id")]
    [Required(ErrorMessage = AppointmentMessages.DOCTOR_ID_IS_REQUIRED)]
    public string? DoctorId { get; set; }
}
