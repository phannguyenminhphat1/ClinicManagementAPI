using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class PaymentDetail
{
    public int PaymentDetailId { get; set; }

    public int? PaymentId { get; set; }

    public int? BillingDetailId { get; set; }

    public int? PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public int? BillingMedicineId { get; set; }

    public virtual BillingDetail? BillingDetail { get; set; }

    public virtual BillingMedicine? BillingMedicine { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual PaymentMethod? PaymentMethod { get; set; }
}
