using LivestockLink.Core.Enums;

namespace LivestockLink.Core.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;

    public Guid RestaurantProfileId { get; set; }
    public RestaurantProfile RestaurantProfile { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPriceAtOrder { get; set; }
    public decimal TotalAmount { get; set; } // Quantity * UnitPriceAtOrder

    public OrderStatus Status { get; set; } = OrderStatus.Submitted;

    public DateTime PreferredDeliveryDate { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryState { get; set; } = string.Empty;

    public string? RestaurantNotes { get; set; }
    public string? SupplierNotes { get; set; }
    public string? DispatchNotes { get; set; }
    public string? CourierContact { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public CommissionRecord? CommissionRecord { get; set; }
    public Dispute? Dispute { get; set; }
}
