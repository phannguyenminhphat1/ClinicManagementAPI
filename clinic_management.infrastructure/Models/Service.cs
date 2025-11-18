using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<BillingDetail> BillingDetails { get; set; } = new List<BillingDetail>();

    public virtual ICollection<MedicalTest> MedicalTests { get; set; } = new List<MedicalTest>();
}
