using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using LivestockLink.Core.Interfaces;
using LivestockLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LivestockLink.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICommissionService _commissionService;

    public OrderService(ApplicationDbContext context, ICommissionService commissionService)
    {
        _context = context;
        _commissionService = commissionService;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid restaurantProfileId, CreateOrderRequest request)
    {
        var restaurant = await _context.RestaurantProfiles
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == restaurantProfileId);

        if (restaurant == null)
        {
            throw new InvalidOperationException("Restaurant profile not found.");
        }

        var product = await _context.Products
            .Include(p => p.SupplierProfile)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product == null || !product.IsActive)
        {
            throw new KeyNotFoundException("Product is unavailable or deactivated.");
        }

        if (!product.SupplierProfile.IsApproved)
        {
            throw new InvalidOperationException("This supplier is currently suspended or under platform review.");
        }

        if (request.Quantity < product.MinimumOrderQuantity)
        {
            throw new ArgumentException($"Requested quantity ({request.Quantity} {product.Unit}) is below the supplier's minimum order requirement of {product.MinimumOrderQuantity} {product.Unit}.");
        }

        if (request.Quantity > product.AvailableQuantity)
        {
            throw new ArgumentException($"Requested quantity ({request.Quantity} {product.Unit}) exceeds available stock ({product.AvailableQuantity} {product.Unit}).");
        }

        if (request.PreferredDeliveryDate.Date < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Preferred delivery date cannot be in the past.");
        }

        // Deduct available quantity upon order submission
        product.AvailableQuantity -= request.Quantity;

        // Generate human-readable order number e.g. LL-2026-1045
        var randomSuffix = Random.Shared.Next(1000, 9999);
        var orderNumber = $"LL-{DateTime.UtcNow:yyyyMMdd}-{randomSuffix}";

        // Critical: Price is snapped server-side from product entity. Client cannot tamper with price.
        var unitPrice = product.PricePerUnit;
        var totalAmount = request.Quantity * unitPrice;

        var order = new Order
        {
            OrderNumber = orderNumber,
            RestaurantProfileId = restaurantProfileId,
            RestaurantProfile = restaurant,
            ProductId = product.Id,
            Product = product,
            Quantity = request.Quantity,
            UnitPriceAtOrder = unitPrice,
            TotalAmount = totalAmount,
            Status = OrderStatus.Submitted,
            PreferredDeliveryDate = DateTime.SpecifyKind(request.PreferredDeliveryDate, DateTimeKind.Utc),
            DeliveryAddress = request.DeliveryAddress.Trim(),
            DeliveryCity = request.DeliveryCity.Trim(),
            DeliveryState = request.DeliveryState.Trim(),
            RestaurantNotes = request.RestaurantNotes?.Trim(),
            SubmittedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetRestaurantOrdersAsync(Guid restaurantProfileId)
    {
        var orders = await _context.Orders
            .Include(o => o.RestaurantProfile).ThenInclude(r => r.User)
            .Include(o => o.Product).ThenInclude(p => p.SupplierProfile).ThenInclude(s => s.User)
            .Include(o => o.CommissionRecord)
            .Include(o => o.Dispute)
            .Where(o => o.RestaurantProfileId == restaurantProfileId)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetSupplierOrdersAsync(Guid supplierProfileId)
    {
        var orders = await _context.Orders
            .Include(o => o.RestaurantProfile).ThenInclude(r => r.User)
            .Include(o => o.Product).ThenInclude(p => p.SupplierProfile).ThenInclude(s => s.User)
            .Include(o => o.CommissionRecord)
            .Include(o => o.Dispute)
            .Where(o => o.Product.SupplierProfileId == supplierProfileId)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.RestaurantProfile).ThenInclude(r => r.User)
            .Include(o => o.Product).ThenInclude(p => p.SupplierProfile).ThenInclude(s => s.User)
            .Include(o => o.CommissionRecord)
            .Include(o => o.Dispute)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid requestingUserId, string role)
    {
        var order = await _context.Orders
            .Include(o => o.RestaurantProfile).ThenInclude(r => r.User)
            .Include(o => o.Product).ThenInclude(p => p.SupplierProfile).ThenInclude(s => s.User)
            .Include(o => o.CommissionRecord)
            .Include(o => o.Dispute)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        // Authorization check: User can only see order if they are Admin, or the Restaurant Buyer, or the Product Supplier
        if (role != UserRole.Admin.ToString())
        {
            var isBuyer = order.RestaurantProfile.UserId == requestingUserId;
            var isSupplier = order.Product.SupplierProfile.UserId == requestingUserId;

            if (!isBuyer && !isSupplier)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this order.");
            }
        }

        return MapToDto(order);
    }

    public async Task<OrderDto> CancelOrderAsync(Guid restaurantProfileId, Guid orderId, CancelOrderRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Product)
            .Include(o => o.RestaurantProfile).ThenInclude(r => r.User)
            .Include(o => o.Product).ThenInclude(p => p.SupplierProfile).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.RestaurantProfileId == restaurantProfileId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found or unauthorized.");
        }

        // Restaurant can only cancel before accepted
        if (order.Status != OrderStatus.Submitted)
        {
            throw new InvalidOperationException($"Cannot cancel order in '{order.Status}' status. Orders can only be cancelled while in 'Submitted' status before supplier acceptance.");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = request.Reason.Trim();
        order.CancelledAt = DateTime.UtcNow;

        // Restore available quantity back to product inventory
        order.Product.AvailableQuantity += order.Quantity;

        await _context.SaveChangesAsync();
        return MapToDto(order);
    }

    public async Task<OrderDto> AcceptOrderAsync(Guid supplierProfileId, Guid orderId, AcceptOrderRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.RestaurantProfile).ThenInclude(r => r.User)
            .Include(o => o.Product).ThenInclude(p => p.SupplierProfile).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Product.SupplierProfileId == supplierProfileId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found or unauthorized for this supplier.");
        }

        if (order.Status != OrderStatus.Submitted)
        {
            throw new InvalidOperationException($"Cannot accept order in '{order.Status}' status. Order must be 'Submitted'.");
        }

        order.Status = OrderStatus.Accepted;
        order.SupplierNotes = request.SupplierNotes?.Trim();
        order.AcceptedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(order);
    }

    public async Task<OrderDto> RejectOrderAsync(Guid supplierProfileId, Guid orderId, RejectOrderRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Product)
            .Include(o => o.RestaurantProfile).ThenInclude(r => r.User)
            .Include(o => o.Product).ThenInclude(p => p.SupplierProfile).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Product.SupplierProfileId == supplierProfileId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found or unauthorized for this supplier.");
        }

        if (order.Status != OrderStatus.Submitted)
        {
            throw new InvalidOperationException($"Cannot reject order in '{order.Status}' status. Order must be 'Submitted'.");
        }

        order.Status = OrderStatus.Rejected;
        order.RejectionReason = request.Reason.Trim();

        // Restore product inventory
        order.Product.AvailableQuantity += order.Quantity;

        await _context.SaveChangesAsync();
        return MapToDto(order);
    }

    public async Task<OrderDto> DispatchOrderAsync(Guid supplierProfileId, Guid orderId, DispatchOrderRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.RestaurantProfile).ThenInclude(r => r.User)
            .Include(o => o.Product).ThenInclude(p => p.SupplierProfile).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Product.SupplierProfileId == supplierProfileId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found or unauthorized for this supplier.");
        }

        if (order.Status != OrderStatus.Accepted)
        {
            throw new InvalidOperationException($"Cannot dispatch order in '{order.Status}' status. Order must be 'Accepted' first.");
        }

        order.Status = OrderStatus.Dispatched;
        order.DispatchNotes = request.DispatchNotes.Trim();
        order.CourierContact = request.CourierContact.Trim();
        order.DispatchedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(order);
    }

    public async Task<OrderDto> ConfirmDeliveryAsync(Guid restaurantProfileId, Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.RestaurantProfile).ThenInclude(r => r.User)
            .Include(o => o.Product).ThenInclude(p => p.SupplierProfile).ThenInclude(s => s.User)
            .Include(o => o.CommissionRecord)
            .Include(o => o.Dispute)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.RestaurantProfileId == restaurantProfileId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found or unauthorized for this restaurant.");
        }

        if (order.Status != OrderStatus.Dispatched)
        {
            throw new InvalidOperationException($"Cannot confirm delivery for order in '{order.Status}' status. Order must be 'Dispatched'.");
        }

        // Transition to Completed
        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Calculate and persist commission record
        var commission = await _commissionService.CalculateAndRecordCommissionAsync(order.Id);
        order.CommissionRecord = commission;

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order o)
    {
        return new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            RestaurantProfileId = o.RestaurantProfileId,
            RestaurantName = o.RestaurantProfile != null ? o.RestaurantProfile.RestaurantName : string.Empty,
            RestaurantContactPerson = o.RestaurantProfile != null ? o.RestaurantProfile.ContactPerson : string.Empty,
            RestaurantPhone = o.RestaurantProfile?.User?.PhoneNumber ?? string.Empty,
            ProductId = o.ProductId,
            ProductTitle = o.Product != null ? o.Product.Title : string.Empty,
            ProductCategory = o.Product != null ? o.Product.Category : ProductCategory.Poultry,
            ProductForm = o.Product != null ? o.Product.ProductForm : ProductForm.LiveAnimal,
            Unit = o.Product != null ? o.Product.Unit : string.Empty,
            SupplierProfileId = o.Product != null ? o.Product.SupplierProfileId : Guid.Empty,
            SupplierName = o.Product?.SupplierProfile != null ? o.Product.SupplierProfile.BusinessName : string.Empty,
            SupplierPhone = o.Product?.SupplierProfile?.User?.PhoneNumber ?? string.Empty,
            Quantity = o.Quantity,
            UnitPriceAtOrder = o.UnitPriceAtOrder,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            PreferredDeliveryDate = o.PreferredDeliveryDate,
            DeliveryAddress = o.DeliveryAddress,
            DeliveryCity = o.DeliveryCity,
            DeliveryState = o.DeliveryState,
            RestaurantNotes = o.RestaurantNotes,
            SupplierNotes = o.SupplierNotes,
            DispatchNotes = o.DispatchNotes,
            CourierContact = o.CourierContact,
            RejectionReason = o.RejectionReason,
            CancellationReason = o.CancellationReason,
            SubmittedAt = o.SubmittedAt,
            AcceptedAt = o.AcceptedAt,
            DispatchedAt = o.DispatchedAt,
            CompletedAt = o.CompletedAt,
            CancelledAt = o.CancelledAt,
            Commission = o.CommissionRecord != null ? new CommissionRecordDto
            {
                Id = o.CommissionRecord.Id,
                OrderId = o.CommissionRecord.OrderId,
                TotalOrderValue = o.CommissionRecord.TotalOrderValue,
                CommissionPercentage = o.CommissionRecord.CommissionPercentage,
                CommissionAmount = o.CommissionRecord.CommissionAmount,
                SupplierNetPayout = o.CommissionRecord.SupplierNetPayout,
                CalculatedAt = o.CommissionRecord.CalculatedAt,
                Status = o.CommissionRecord.Status,
                Notes = o.CommissionRecord.Notes
            } : null,
            Dispute = o.Dispute != null ? new DisputeDto
            {
                Id = o.Dispute.Id,
                OrderId = o.Dispute.OrderId,
                OrderNumber = o.OrderNumber,
                RaisedByUserId = o.Dispute.RaisedByUserId,
                Reason = o.Dispute.Reason,
                Description = o.Dispute.Description,
                Status = o.Dispute.Status,
                ResolutionNotes = o.Dispute.ResolutionNotes,
                RaisedAt = o.Dispute.RaisedAt,
                ResolvedAt = o.Dispute.ResolvedAt
            } : null
        };
    }
}
