using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class MakePaymentDto
{
    [JsonPropertyName("payment_method_id")]
    [Required(ErrorMessage = BillingMessages.PAYMENT_METHOD_ID_IS_REQUIRED)]
    public string? PaymentMethodId { get; set; }

}