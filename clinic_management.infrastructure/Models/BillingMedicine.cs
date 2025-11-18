using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class BillingMedicine
{
    public int BillingMedicineId { get; set; }

    public int? BillingId { get; set; }

    public int? MedicineId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public int? PaymentStatusId { get; set; }

    public virtual Billing? Billing { get; set; }

    public virtual Medicine? Medicine { get; set; }

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

    public virtual PaymentStatus? PaymentStatus { get; set; }
}
