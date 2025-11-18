using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class Billing
{
    public int BillingId { get; set; }

    public int? AppointmentId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual ICollection<BillingDetail> BillingDetails { get; set; } = new List<BillingDetail>();

    public virtual ICollection<BillingMedicine> BillingMedicines { get; set; } = new List<BillingMedicine>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
