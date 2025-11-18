using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class Prescription
{
    public int PrescriptionId { get; set; }

    public int? MedicalRecordDetailId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual MedicalRecordDetail? MedicalRecordDetail { get; set; }

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
