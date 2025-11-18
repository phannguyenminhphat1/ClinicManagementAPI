using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public Guid? PatientId { get; set; }

    public Guid? DoctorId { get; set; }

    public DateTime ScheduledDate { get; set; }

    public int? StatusId { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AppointmentStatusHistory> AppointmentStatusHistories { get; set; } = new List<AppointmentStatusHistory>();

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();

    public virtual User? Doctor { get; set; }

    public virtual MedicalRecordDetail? MedicalRecordDetail { get; set; }

    public virtual User? Patient { get; set; }

    public virtual Status? Status { get; set; }
}
