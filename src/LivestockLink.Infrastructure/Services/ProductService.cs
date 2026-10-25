using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Interfaces;
using LivestockLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LivestockLink.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(ProductFilterRequest? filter)
    {
        var query = _context.Products
            .Include(p => p.SupplierProfile)
            .AsNoTracking()
            .AsQueryable();

        // Default: only active products from approved suppliers for buyers
        query = query.Where(p => p.IsActive && p.SupplierProfile.IsApproved);

        if (filter != null)
        {
            if (filter.Category.HasValue)
            {
                query = query.Where(p => p.Category == filter.Category.Value);
            }

            if (filter.ProductForm.HasValue)
            {
                query = query.Where(p => p.ProductForm == filter.ProductForm.Value);
            }

            if (filter.SupplierId.HasValue)
            {
                query = query.Where(p => p.SupplierProfileId == filter.SupplierId.Value);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.PricePerUnit >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.PricePerUnit <= filter.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.State))
            {
                var stateLower = filter.State.Trim().ToLower();
                query = query.Where(p => p.SupplierProfile.State.ToLower().Contains(stateLower) ||
                                         p.DeliveryCoverage.ToLower().Contains(stateLower));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                var search = filter.SearchQuery.Trim().ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(search) ||
                                         p.Description.ToLower().Contains(search) ||
                                         p.SupplierProfile.BusinessName.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "price_asc" => query.OrderBy(p => p.PricePerUnit),
                    "price_desc" => query.OrderByDescending(p => p.PricePerUnit),
                    "name" => query.OrderBy(p => p.Title),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
        }
        else
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        var products = await query.ToListAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.SupplierProfile)
            .FirstOrDefaultAsync(p => p.Id == productId);

        return product != null ? MapToDto(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetSupplierProductsAsync(Guid supplierProfileId)
    {
        var products = await _context.Products
            .Include(p => p.SupplierProfile)
            .Where(p => p.SupplierProfileId == supplierProfileId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(Guid supplierProfileId, CreateProductRequest request)
    {
        var supplier = await _context.SupplierProfiles.FindAsync(supplierProfileId);
        if (supplier == null)
        {
            throw new InvalidOperationException("Supplier profile not found.");
        }

        if (request.PricePerUnit <= 0)
        {
            throw new ArgumentException("Product price must be greater than zero.");
        }

        if (request.AvailableQuantity < 0)
        {
            throw new ArgumentException("Available quantity cannot be negative.");
        }

        if (request.MinimumOrderQuantity <= 0)
        {
            throw new ArgumentException("Minimum order quantity must be at least 1.");
        }

        var product = new Product
        {
            SupplierProfileId = supplierProfileId,
            SupplierProfile = supplier,
            Title = request.Title.Trim(),
            Category = request.Category,
            ProductForm = request.ProductForm,
            Unit = request.Unit.Trim(),
            PricePerUnit = request.PricePerUnit,
            AvailableQuantity = request.AvailableQuantity,
            MinimumOrderQuantity = request.MinimumOrderQuantity,
            DeliveryCoverage = request.DeliveryCoverage.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ImageUrl = !string.IsNullOrWhiteSpace(request.ImageUrl) ? request.ImageUrl.Trim() : "/images/placeholder-livestock.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid supplierProfileId, Guid productId, UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.SupplierProfile)
            .FirstOrDefaultAsync(p => p.Id == productId && p.SupplierProfileId == supplierProfileId);

        if (product == null)
        {
            throw new KeyNotFoundException("Product listing not found or not owned by supplier.");
        }

        if (request.PricePerUnit <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.");
        }

        if (request.AvailableQuantity < 0)
        {
            throw new ArgumentException("Available quantity cannot be negative.");
        }

        if (request.MinimumOrderQuantity <= 0)
        {
            throw new ArgumentException("Minimum order quantity must be at least 1.");
        }

        product.Title = request.Title.Trim();
        product.Category = request.Category;
        product.ProductForm = request.ProductForm;
        product.Unit = request.Unit.Trim();
        product.PricePerUnit = request.PricePerUnit;
        product.AvailableQuantity = request.AvailableQuantity;
        product.MinimumOrderQuantity = request.MinimumOrderQuantity;
        product.DeliveryCoverage = request.DeliveryCoverage.Trim();
        product.Description = request.Description?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(request.ImageUrl)) product.ImageUrl = request.ImageUrl.Trim();
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(product);
    }

    public async Task<bool> ToggleProductActiveAsync(Guid supplierProfileId, Guid productId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && p.SupplierProfileId == supplierProfileId);

        if (product == null) return false;

        product.IsActive = !product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return product.IsActive;
    }

    public async Task<bool> DeleteProductAsync(Guid supplierProfileId, Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Orders)
            .FirstOrDefaultAsync(p => p.Id == productId && p.SupplierProfileId == supplierProfileId);

        if (product == null) return false;

        // If product has orders, deactivate instead of hard delete to preserve financial integrity
        if (product.Orders.Any())
        {
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            SupplierProfileId = p.SupplierProfileId,
            SupplierName = p.SupplierProfile != null ? p.SupplierProfile.BusinessName : string.Empty,
            SupplierLocation = p.SupplierProfile != null ? p.SupplierProfile.FarmLocation : string.Empty,
            SupplierState = p.SupplierProfile != null ? p.SupplierProfile.State : string.Empty,
            Title = p.Title,
            Category = p.Category,
            ProductForm = p.ProductForm,
            Unit = p.Unit,
            PricePerUnit = p.PricePerUnit,
            AvailableQuantity = p.AvailableQuantity,
            MinimumOrderQuantity = p.MinimumOrderQuantity,
            DeliveryCoverage = p.DeliveryCoverage,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        };
    }
}
