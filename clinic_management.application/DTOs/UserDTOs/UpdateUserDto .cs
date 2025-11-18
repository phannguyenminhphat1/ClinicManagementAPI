using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UpdateUserDto
{
    [JsonPropertyName("fullname")]
    [RegularExpression(@"^[A-Za-zÀ-ỹ\s]{2,50}$", ErrorMessage = UserMessages.FULL_NAME_IS_INVALID)]
    public string? Fullname { get; set; }

    [JsonPropertyName("gender")]
    [Range(0, 1, ErrorMessage = AuthMessages.GENDER_IS_INVALID)]
    public byte? Gender { get; set; }

    [JsonPropertyName("birth_date")]
    public string? BirthDate { get; set; }

    [JsonPropertyName("email")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = UserMessages.EMAIL_IS_INVALID)]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = UserMessages.PHONE_IS_INVALID)]
    public string? Phone { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("image")]
    [RegularExpression(@"^https?:\/\/.*\.(jpg|jpeg|png|gif|bmp|webp)$", ErrorMessage = UserMessages.IMAGE_IS_INVALID)]
    public string? Image { get; set; }

    [JsonPropertyName("weight")]
    public string? Weight { get; set; }

    // [JsonPropertyName("specialty_id")]
    // public string? SpecialtyId { get; set; }

}
