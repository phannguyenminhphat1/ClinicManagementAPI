using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? BillingId { get; set; }

    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Billing? Billing { get; set; }

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();
}
