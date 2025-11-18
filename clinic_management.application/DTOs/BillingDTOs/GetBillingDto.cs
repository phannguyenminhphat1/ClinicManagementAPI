using System.Text.Json.Serialization;
using clinic_management.infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

public class GetBillingDto
{
    [JsonPropertyName("billing_id")]
    public int BillingId { get; set; }

    [JsonPropertyName("appointment")]
    public GetAppointmentDto? Appointment { get; set; }

    [JsonPropertyName("billing_details")]
    public List<GetBillingDetailDto>? BillingDetails { get; set; }

    [JsonPropertyName("billing_medicines")]
    public List<GetBillingMedicineDto>? BillingMedicines { get; set; }

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("payment_status_name")]
    public string? StatusName { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

