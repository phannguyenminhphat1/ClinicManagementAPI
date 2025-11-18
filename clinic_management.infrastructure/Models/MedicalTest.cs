using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class MedicalTest
{
    public int MedicalTestId { get; set; }

    public int? MedicalRecordDetailId { get; set; }

    public int? ServiceId { get; set; }

    public int? StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual MedicalRecordDetail? MedicalRecordDetail { get; set; }

    public virtual ICollection<MedicalTestResult> MedicalTestResults { get; set; } = new List<MedicalTestResult>();

    public virtual Service? Service { get; set; }

    public virtual Status? Status { get; set; }
}
