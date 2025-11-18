using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class MedicalRecord
{
    public int MedicalRecordId { get; set; }

    public Guid? PatientId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<MedicalRecordDetail> MedicalRecordDetails { get; set; } = new List<MedicalRecordDetail>();

    public virtual User? Patient { get; set; }
}
