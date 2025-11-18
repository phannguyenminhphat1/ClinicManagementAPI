using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class AddPrescriptionDetailDto
{
    [JsonPropertyName("prescription_detail_id")]
    public int? PrescriptionDetailId { get; set; }

    [JsonPropertyName("prescription_id")]
    public string? PrescriptionId { get; set; }

    [JsonPropertyName("medicine_id")]
    [Required(ErrorMessage = PrescriptionMessages.MEDICINE_ID_IS_REQUIRED)]
    public string? MedicineId { get; set; }

    [JsonPropertyName("medicine_name")]
    public string? MedicineName { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("quantity")]
    [Required(ErrorMessage = PrescriptionMessages.QUANTITY_IS_REQUIRED)]
    public string? Quantity { get; set; }

    [JsonPropertyName("usage")]
    [Required(ErrorMessage = PrescriptionMessages.USAGE_IS_REQUIRED)]
    public string? Usage { get; set; }
}