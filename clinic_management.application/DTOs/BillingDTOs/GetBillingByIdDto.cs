using System.Text.Json.Serialization;
using clinic_management.infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

public class GetBillingByIdDto
{
    [JsonPropertyName("billing_id")]
    public int BillingId { get; set; }

    [JsonPropertyName("appointment")]
    public GetAppointmentDto? Appointment { get; set; }

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("list_billing_detail")]
    public List<GetBillingDetailDto>? BillingDetails { get; set; }

    [JsonPropertyName("list_billing_medicine")]
    public List<GetBillingMedicineDto>? BillingMedicines { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

