namespace LivestockLink.Core.Entities;

public class CommissionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal TotalOrderValue { get; set; }
    public decimal CommissionPercentage { get; set; } // e.g. 5.0m
    public decimal CommissionAmount { get; set; }     // e.g. 20,000
    public decimal SupplierNetPayout { get; set; }    // e.g. 380,000

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Recorded"; // Recorded, Invoiced, Settled
    public string? Notes { get; set; }
}
