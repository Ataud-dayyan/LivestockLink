using LivestockLink.Core.DTOs;

namespace LivestockLink.Core.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(ProductFilterRequest? filter);
    Task<ProductDto?> GetProductByIdAsync(Guid productId);
    Task<IEnumerable<ProductDto>> GetSupplierProductsAsync(Guid supplierProfileId);
    Task<ProductDto> CreateProductAsync(Guid supplierProfileId, CreateProductRequest request);
    Task<ProductDto> UpdateProductAsync(Guid supplierProfileId, Guid productId, UpdateProductRequest request);
    Task<bool> ToggleProductActiveAsync(Guid supplierProfileId, Guid productId);
    Task<bool> DeleteProductAsync(Guid supplierProfileId, Guid productId);
}
