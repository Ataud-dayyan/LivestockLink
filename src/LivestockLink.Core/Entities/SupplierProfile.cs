namespace LivestockLink.Core.Entities;

public class SupplierProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string BusinessName { get; set; } = string.Empty;
    public string FarmLocation { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string DeliveryCoverage { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
