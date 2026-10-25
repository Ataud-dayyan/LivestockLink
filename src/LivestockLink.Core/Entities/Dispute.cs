using LivestockLink.Core.Enums;

namespace LivestockLink.Core.Entities;

public class Dispute
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid RaisedByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public DisputeStatus Status { get; set; } = DisputeStatus.Pending;
    public string? ResolutionNotes { get; set; }
    public Guid? ResolvedByUserId { get; set; }

    public DateTime RaisedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
