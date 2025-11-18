using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public class DateTimeDto
{
    [JsonPropertyName("date")]
    [Required(ErrorMessage = AppointmentMessages.DATE_IS_REQUIRED)]
    public string? Date { get; set; }
}
