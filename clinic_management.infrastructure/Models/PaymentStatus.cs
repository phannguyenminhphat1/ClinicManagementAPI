using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class PaymentStatus
{
    public int PaymentStatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<BillingDetail> BillingDetails { get; set; } = new List<BillingDetail>();

    public virtual ICollection<BillingMedicine> BillingMedicines { get; set; } = new List<BillingMedicine>();
}
