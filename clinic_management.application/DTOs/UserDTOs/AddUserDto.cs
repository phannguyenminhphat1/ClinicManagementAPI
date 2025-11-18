using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class AddUserDto
{
    [JsonPropertyName("fullname")]
    [Required(ErrorMessage = UserMessages.FULL_NAME_IS_REQUIRED)]
    [RegularExpression(@"^[A-Za-zÀ-ỹ\s]{2,50}$", ErrorMessage = UserMessages.FULL_NAME_IS_INVALID)]
    public string Fullname { get; set; } = null!;

    [JsonPropertyName("gender")]
    [Required(ErrorMessage = AuthMessages.GENDER_IS_REQUIRED)]
    [Range(0, 1, ErrorMessage = AuthMessages.GENDER_IS_INVALID)]
    public byte? Gender { get; set; }

    [JsonPropertyName("birth_date")]
    [Required(ErrorMessage = UserMessages.BIRTH_DATE_IS_REQUIRED)]
    public string? BirthDate { get; set; }

    [JsonPropertyName("email")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = UserMessages.EMAIL_IS_INVALID)]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    [Required(ErrorMessage = UserMessages.PHONE_IS_REQUIRED)]
    [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = UserMessages.PHONE_IS_INVALID)]
    public string? Phone { get; set; }

    [JsonPropertyName("address")]
    [Required(ErrorMessage = UserMessages.ADDRESS_IS_REQUIRED)]
    [RegularExpression(@"^[A-Za-zÀ-ỹ0-9\s,.\-]{2,100}$", ErrorMessage = UserMessages.ADDRESS_IS_INVALID)]
    public string? Address { get; set; }

    public string? Image { get; set; }

    [JsonPropertyName("role_id")]
    [Required(ErrorMessage = UserMessages.ROLE_IS_REQUIRED)]
    public string? RoleId { get; set; }

    [JsonPropertyName("specialty_id")]
    public string? SpecialtyId { get; set; }

}
