using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using LivestockLink.Core.Interfaces;
using LivestockLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LivestockLink.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly ICommissionService _commissionService;

    public AdminService(ApplicationDbContext context, ICommissionService commissionService)
    {
        _context = context;
        _commissionService = commissionService;
    }

    public async Task<AdminDashboardMetricsDto> GetDashboardMetricsAsync()
    {
        var orders = await _context.Orders.AsNoTracking().ToListAsync();
        var commissions = await _context.CommissionRecords.AsNoTracking().ToListAsync();
        var currentRate = await _commissionService.GetCurrentCommissionRateAsync();

        var totalVolume = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount);
        var totalCommission = commissions.Sum(c => c.CommissionAmount);

        var suppliersCount = await _context.SupplierProfiles.CountAsync();
        var restaurantsCount = await _context.RestaurantProfiles.CountAsync();
        var activeProducts = await _context.Products.CountAsync(p => p.IsActive);

        return new AdminDashboardMetricsDto
        {
            TotalTransactionVolume = totalVolume,
            TotalCommissionRevenue = totalCommission,
            CurrentCommissionRate = currentRate,
            TotalOrdersCount = orders.Count,
            CompletedOrdersCount = orders.Count(o => o.Status == OrderStatus.Completed),
            PendingOrdersCount = orders.Count(o => o.Status == OrderStatus.Submitted),
            DispatchedOrdersCount = orders.Count(o => o.Status == OrderStatus.Dispatched),
            DisputedOrdersCount = orders.Count(o => o.Status == OrderStatus.Disputed),
            CancelledOrdersCount = orders.Count(o => o.Status == OrderStatus.Cancelled || o.Status == OrderStatus.Rejected),
            TotalSuppliersCount = suppliersCount,
            TotalRestaurantsCount = restaurantsCount,
            ActiveProductsCount = activeProducts
        };
    }

    public async Task<IEnumerable<SupplierSummaryDto>> GetAllSuppliersAsync()
    {
        var suppliers = await _context.SupplierProfiles
            .Include(s => s.User)
            .Include(s => s.Products)
                .ThenInclude(p => p.Orders)
                    .ThenInclude(o => o.CommissionRecord)
            .AsNoTracking()
            .ToListAsync();

        return suppliers.Select(s =>
        {
            var allSupplierOrders = s.Products.SelectMany(p => p.Orders).ToList();
            var completedOrders = allSupplierOrders.Where(o => o.Status == OrderStatus.Completed).ToList();
            var grossSales = completedOrders.Sum(o => o.TotalAmount);
            var commissionDeducted = completedOrders.Where(o => o.CommissionRecord != null).Sum(o => o.CommissionRecord!.CommissionAmount);
            var netEarnings = grossSales - commissionDeducted;

            return new SupplierSummaryDto
            {
                Id = s.Id,
                UserId = s.UserId,
                BusinessName = s.BusinessName,
                ContactPerson = s.User.FullName,
                Email = s.User.Email,
                Phone = s.User.PhoneNumber,
                Location = s.FarmLocation,
                State = s.State,
                DeliveryCoverage = s.DeliveryCoverage,
                IsApproved = s.IsApproved,
                IsActive = s.User.IsActive,
                ProductCount = s.Products.Count,
                TotalOrdersFulfilled = completedOrders.Count,
                TotalGrossSales = grossSales,
                TotalCommissionDeducted = commissionDeducted,
                TotalNetEarnings = netEarnings,
                CreatedAt = s.CreatedAt
            };
        });
    }

    public async Task<IEnumerable<RestaurantSummaryDto>> GetAllRestaurantsAsync()
    {
        var restaurants = await _context.RestaurantProfiles
            .Include(r => r.User)
            .Include(r => r.Orders)
            .AsNoTracking()
            .ToListAsync();

        return restaurants.Select(r =>
        {
            var totalOrders = r.Orders.Count;
            var totalSpend = r.Orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount);

            return new RestaurantSummaryDto
            {
                Id = r.Id,
                UserId = r.UserId,
                RestaurantName = r.RestaurantName,
                ContactPerson = r.ContactPerson,
                Email = r.User.Email,
                Phone = r.User.PhoneNumber,
                City = r.City,
                State = r.State,
                Address = r.BusinessAddress,
                IsActive = r.User.IsActive,
                TotalOrdersPlaced = totalOrders,
                TotalSpend = totalSpend,
                CreatedAt = r.CreatedAt
            };
        });
    }

    public async Task<bool> SetSupplierApprovalAsync(Guid supplierProfileId, bool isApproved)
    {
        var supplier = await _context.SupplierProfiles.FindAsync(supplierProfileId);
        if (supplier == null) return false;

        supplier.IsApproved = isApproved;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }
}
