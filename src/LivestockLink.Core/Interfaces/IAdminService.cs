using LivestockLink.Core.DTOs;

namespace LivestockLink.Core.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardMetricsDto> GetDashboardMetricsAsync();
    Task<IEnumerable<SupplierSummaryDto>> GetAllSuppliersAsync();
    Task<IEnumerable<RestaurantSummaryDto>> GetAllRestaurantsAsync();
    Task<bool> SetSupplierApprovalAsync(Guid supplierProfileId, bool isApproved);
    Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive);
}
