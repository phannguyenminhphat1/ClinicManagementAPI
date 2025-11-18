using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UploadFileDto
{
    [JsonPropertyName("image")]
    [Required(ErrorMessage = UserMessages.IMAGE_IS_REQUIRED)]
    public IFormFile? File { get; set; }
}
