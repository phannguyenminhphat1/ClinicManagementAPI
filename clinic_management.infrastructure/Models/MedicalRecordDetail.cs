using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class MedicalRecordDetail
{
    public int MedicalRecordDetailId { get; set; }

    public int? MedicalRecordId { get; set; }

    public int? AppointmentId { get; set; }

    public string Symptoms { get; set; } = null!;

    public string? Diagnosis { get; set; }

    public string? Notes { get; set; }

    public bool RequiresTest { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual MedicalRecord? MedicalRecord { get; set; }

    public virtual ICollection<MedicalTest> MedicalTests { get; set; } = new List<MedicalTest>();

    public virtual Prescription? Prescription { get; set; }
}
