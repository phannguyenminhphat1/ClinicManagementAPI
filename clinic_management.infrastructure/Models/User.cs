using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string? Fullname { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public byte? Gender { get; set; }

    public DateOnly BirthDate { get; set; }

    public string? Phone { get; set; }

    public string? Image { get; set; }

    public string? Address { get; set; }

    public double? Weight { get; set; }

    public int? SpecialtyId { get; set; }

    public int? RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Appointment> AppointmentDoctors { get; set; } = new List<Appointment>();

    public virtual ICollection<Appointment> AppointmentPatients { get; set; } = new List<Appointment>();

    public virtual ICollection<Conversation> ConversationUser1s { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> ConversationUser2s { get; set; } = new List<Conversation>();

    public virtual MedicalRecord? MedicalRecord { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Role? Role { get; set; }

    public virtual Specialty? Specialty { get; set; }
}
