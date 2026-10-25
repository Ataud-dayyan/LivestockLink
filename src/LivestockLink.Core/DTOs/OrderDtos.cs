using System.ComponentModel.DataAnnotations;
using LivestockLink.Core.Enums;

namespace LivestockLink.Core.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public Guid RestaurantProfileId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string RestaurantContactPerson { get; set; } = string.Empty;
    public string RestaurantPhone { get; set; } = string.Empty;

    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public ProductCategory ProductCategory { get; set; }
    public ProductForm ProductForm { get; set; }
    public string Unit { get; set; } = string.Empty;

    public Guid SupplierProfileId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierPhone { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPriceAtOrder { get; set; }
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }
    public string StatusName => Status.ToString();

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

    public DateTime SubmittedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Commission summary (visible to Supplier and Admin)
    public CommissionRecordDto? Commission { get; set; }
    public DisputeDto? Dispute { get; set; }
}

public class CreateOrderRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required, Range(1, 1000000, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [Required]
    public DateTime PreferredDeliveryDate { get; set; }

    [Required, MaxLength(250)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string DeliveryCity { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string DeliveryState { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RestaurantNotes { get; set; }
}

public class AcceptOrderRequest
{
    [MaxLength(500)]
    public string? SupplierNotes { get; set; }
}

public class RejectOrderRequest
{
    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class DispatchOrderRequest
{
    [Required, MaxLength(300)]
    public string DispatchNotes { get; set; } = string.Empty; // e.g. "Loaded in refrigerated van LAG-442-XY"

    [Required, MaxLength(100)]
    public string CourierContact { get; set; } = string.Empty; // Driver name and phone
}

public class CancelOrderRequest
{
    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
