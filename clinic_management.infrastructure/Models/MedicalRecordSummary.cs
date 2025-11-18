using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class MedicalRecordSummary
{
    public Guid PatientId { get; set; }

    public string? PatientName { get; set; }

    public byte? Gender { get; set; }

    public DateOnly BirthDate { get; set; }

    public int MedicalRecordId { get; set; }

    public int MedicalRecordDetailId { get; set; }

    public int AppointmentId { get; set; }

    public DateTime ScheduledDate { get; set; }

    public string StatusName { get; set; } = null!;

    public Guid? DoctorId { get; set; }

    public string? DoctorName { get; set; }

    public string? SpecialtyName { get; set; }

    public string Symptoms { get; set; } = null!;

    public string? Diagnosis { get; set; }

    public string? Notes { get; set; }

    public bool RequiresTest { get; set; }

    public string? Tests { get; set; }

    public string? Prescriptions { get; set; }
}
