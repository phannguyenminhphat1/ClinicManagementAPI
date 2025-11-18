using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class Specialty
{
    public int SpecialtyId { get; set; }

    public string SpecialtyName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
