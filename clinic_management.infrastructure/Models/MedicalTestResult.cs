using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class MedicalTestResult
{
    public int MedicalTestResultId { get; set; }

    public int? MedicalTestId { get; set; }

    public string Parameter { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string? Unit { get; set; }

    public string? ReferenceRange { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Image { get; set; }

    public virtual MedicalTest? MedicalTest { get; set; }
}
