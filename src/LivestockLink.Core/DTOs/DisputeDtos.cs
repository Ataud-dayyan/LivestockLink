using System.ComponentModel.DataAnnotations;
using LivestockLink.Core.Enums;

namespace LivestockLink.Core.DTOs;

public class DisputeDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid RaisedByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? ResolutionNotes { get; set; }
    public DateTime RaisedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CreateDisputeRequest
{
    [Required, MaxLength(150)]
    public string Reason { get; set; } = string.Empty; // e.g. "Damaged cargo", "Quantity short by 10 birds", "Spoiled catfish"

    [Required, MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}

public class ResolveDisputeRequest
{
    [Required]
    public DisputeStatus Status { get; set; } // ResolvedRefunded, ResolvedCompleted, Rejected

    [Required, MaxLength(1000)]
    public string ResolutionNotes { get; set; } = string.Empty;
}
