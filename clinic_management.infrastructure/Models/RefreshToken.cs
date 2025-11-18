using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class RefreshToken
{
    public Guid RefreshTokenId { get; set; }

    public string? Token { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiresTime { get; set; }

    public virtual User? User { get; set; }
}
