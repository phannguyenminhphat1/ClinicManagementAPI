using System;
using System.Collections.Generic;

namespace clinic_management.infrastructure.Models;

public partial class Conversation
{
    public int ConversationId { get; set; }

    public Guid User1Id { get; set; }

    public Guid User2Id { get; set; }

    public int? LastMessageId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid UserMinId { get; set; }

    public Guid UserMaxId { get; set; }

    public virtual Message? LastMessage { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User1 { get; set; } = null!;

    public virtual User User2 { get; set; } = null!;
}
