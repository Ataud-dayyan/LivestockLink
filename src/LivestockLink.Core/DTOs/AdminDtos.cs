using LivestockLink.Core.Enums;

namespace LivestockLink.Core.DTOs;

public class AdminDashboardMetricsDto
{
    public decimal TotalTransactionVolume { get; set; } // ₦ Total completed order gross
    public decimal TotalCommissionRevenue { get; set; } // ₦ Platform commission earned
    public decimal CurrentCommissionRate { get; set; }  // e.g. 5.0%
    public int TotalOrdersCount { get; set; }
    public int CompletedOrdersCount { get; set; }
    public int PendingOrdersCount { get; set; }
    public int DispatchedOrdersCount { get; set; }
    public int DisputedOrdersCount { get; set; }
    public int CancelledOrdersCount { get; set; }
    public int TotalSuppliersCount { get; set; }
    public int TotalRestaurantsCount { get; set; }
    public int ActiveProductsCount { get; set; }
}

public class SupplierApprovalRequest
{
    public bool IsApproved { get; set; }
}

public class UserStatusUpdateRequest
{
    public bool IsActive { get; set; }
}

public class SupplierSummaryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string DeliveryCoverage { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public int TotalOrdersFulfilled { get; set; }
    public decimal TotalGrossSales { get; set; }
    public decimal TotalCommissionDeducted { get; set; }
    public decimal TotalNetEarnings { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RestaurantSummaryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalOrdersPlaced { get; set; }
    public decimal TotalSpend { get; set; }
    public DateTime CreatedAt { get; set; }
}
