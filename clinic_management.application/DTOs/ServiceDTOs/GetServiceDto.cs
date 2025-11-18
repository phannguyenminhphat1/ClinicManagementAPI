using System.Text.Json.Serialization;

public class GetServiceDto
{
    [JsonPropertyName("service_id")]
    public int ServiceId { get; set; }

    [JsonPropertyName("service_name")]
    public string ServiceName { get; set; } = null!;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

}