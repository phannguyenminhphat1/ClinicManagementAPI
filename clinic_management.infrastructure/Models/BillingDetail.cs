using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class BillingDetail
{
    public int BillingDetailId { get; set; }

    public int? BillingId { get; set; }

    public int? ServiceId { get; set; }

    public decimal Price { get; set; }

    public int? PaymentStatusId { get; set; }

    public virtual Billing? Billing { get; set; }

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

    public virtual PaymentStatus? PaymentStatus { get; set; }

    public virtual Service? Service { get; set; }
}
