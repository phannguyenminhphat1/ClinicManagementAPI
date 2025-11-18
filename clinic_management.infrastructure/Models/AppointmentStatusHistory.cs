using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class AppointmentStatusHistory
{
    public int HistoryId { get; set; }

    public int AppointmentId { get; set; }

    public int StatusId { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;
}
