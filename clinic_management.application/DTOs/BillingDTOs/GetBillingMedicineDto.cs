using System.Text.Json.Serialization;

public class GetBillingMedicineDto
{
    [JsonPropertyName("billing_medicine_id")]
    public int BillingMedicineId { get; set; }

    [JsonPropertyName("medicine_name")]
    public string? MedicineName { get; set; }

    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("payment_status_name")]
    public string? StatusName { get; set; }
}
