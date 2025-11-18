using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UserRegisterDto
{
    [JsonPropertyName("phone")]
    [Required(ErrorMessage = AuthMessages.PHONE_IS_REQUIRED)]
    [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = UserMessages.PHONE_IS_INVALID)]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("fullname")]
    [Required(ErrorMessage = AuthMessages.FULLNAME_IS_REQUIRED)]
    [RegularExpression(@"^[A-Za-zÀ-ỹ\s]{2,50}$", ErrorMessage = UserMessages.FULL_NAME_IS_INVALID)]
    public string Fullname { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    [Required(ErrorMessage = AuthMessages.GENDER_IS_REQUIRED)]
    [Range(0, 1, ErrorMessage = AuthMessages.GENDER_IS_INVALID)]
    public byte? Gender { get; set; }

    [JsonPropertyName("birth_date")]
    [Required(ErrorMessage = AuthMessages.BIRTH_DATE_IS_REQUIRED)]
    public string? BirthDate { get; set; }

    [JsonPropertyName("date")]
    [Required(ErrorMessage = AppointmentMessages.DATE_IS_REQUIRED)]
    public string? Date { get; set; } // Chỉ ngày

    [JsonPropertyName("time")]
    [Required(ErrorMessage = AppointmentMessages.TIME_IS_REQUIRED)]
    public string Time { get; set; } = string.Empty; // "08:30" format

    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;


}