using System.Text.Json.Serialization;

public class GetBillingDetailDto
{
    [JsonPropertyName("billing_detail_id")]
    public int BillingDetailId { get; set; }

    [JsonPropertyName("service_name")]
    public string? ServiceName { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("payment_status_name")]
    public string? StatusName { get; set; }
}
