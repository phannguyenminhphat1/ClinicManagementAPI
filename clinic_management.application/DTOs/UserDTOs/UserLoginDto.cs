
using System.ComponentModel.DataAnnotations;

public class UserLoginDto
{
    [Required(ErrorMessage = AuthMessages.PHONE_IS_REQUIRED)]
    public string Phone { get; set; } = string.Empty;

    // public string Password { get; set; } = string.Empty;



}