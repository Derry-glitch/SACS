using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class ChatSession : BaseEntity
{
    public long UserId { get; set; }
    public string Title { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<AIInteraction> AIInteractions { get; set; } = new List<AIInteraction>();
}
