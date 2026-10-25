using System.ComponentModel.DataAnnotations;
using LivestockLink.Core.Enums;

namespace LivestockLink.Core.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid SupplierProfileId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierLocation { get; set; } = string.Empty;
    public string SupplierState { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public ProductForm ProductForm { get; set; }
    public string ProductFormName => ProductForm.ToString();
    public string Unit { get; set; } = string.Empty;
    public decimal PricePerUnit { get; set; }
    public int AvailableQuantity { get; set; }
    public int MinimumOrderQuantity { get; set; }
    public string DeliveryCoverage { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProductRequest
{
    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public ProductCategory Category { get; set; }

    [Required]
    public ProductForm ProductForm { get; set; }

    [Required, MaxLength(30)]
    public string Unit { get; set; } = "Kg";

    [Required, Range(1, 100000000, ErrorMessage = "Price must be greater than zero.")]
    public decimal PricePerUnit { get; set; }

    [Required, Range(1, 1000000, ErrorMessage = "Available quantity must be positive.")]
    public int AvailableQuantity { get; set; }

    [Required, Range(1, 100000, ErrorMessage = "Minimum order must be at least 1.")]
    public int MinimumOrderQuantity { get; set; } = 1;

    [Required, MaxLength(250)]
    public string DeliveryCoverage { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
}

public class UpdateProductRequest
{
    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public ProductCategory Category { get; set; }

    [Required]
    public ProductForm ProductForm { get; set; }

    [Required, MaxLength(30)]
    public string Unit { get; set; } = "Kg";

    [Required, Range(1, 100000000)]
    public decimal PricePerUnit { get; set; }

    [Required, Range(0, 1000000)]
    public int AvailableQuantity { get; set; }

    [Required, Range(1, 100000)]
    public int MinimumOrderQuantity { get; set; }

    [Required, MaxLength(250)]
    public string DeliveryCoverage { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

public class ProductFilterRequest
{
    public ProductCategory? Category { get; set; }
    public ProductForm? ProductForm { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchQuery { get; set; }
    public string? State { get; set; }
    public string? SortBy { get; set; } // "price_asc", "price_desc", "name", "newest"
}
