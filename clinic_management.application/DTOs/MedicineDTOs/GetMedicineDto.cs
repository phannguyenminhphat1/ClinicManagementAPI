using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class GetMedicineDto
{
    [JsonPropertyName("medicine_id")]
    public string? MedicineId { get; set; }

    [JsonPropertyName("medicine_name")]
    public string? MedicineName { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("stock")]
    public int Stock { get; set; }
}