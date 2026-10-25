using LivestockLink.Core.Enums;

namespace LivestockLink.Core.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SupplierProfileId { get; set; }
    public SupplierProfile SupplierProfile { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public ProductForm ProductForm { get; set; }
    public string Unit { get; set; } = "Kg"; // e.g., "Bird", "Kg", "Head", "Crate", "Bag"
    public decimal PricePerUnit { get; set; } // In NGN (₦)
    public int AvailableQuantity { get; set; }
    public int MinimumOrderQuantity { get; set; } = 1;
    public string DeliveryCoverage { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
