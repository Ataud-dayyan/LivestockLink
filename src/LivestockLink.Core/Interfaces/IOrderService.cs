using LivestockLink.Core.DTOs;

namespace LivestockLink.Core.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid restaurantProfileId, CreateOrderRequest request);
    Task<IEnumerable<OrderDto>> GetRestaurantOrdersAsync(Guid restaurantProfileId);
    Task<IEnumerable<OrderDto>> GetSupplierOrdersAsync(Guid supplierProfileId);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid requestingUserId, string role);
    Task<OrderDto> CancelOrderAsync(Guid restaurantProfileId, Guid orderId, CancelOrderRequest request);
    Task<OrderDto> AcceptOrderAsync(Guid supplierProfileId, Guid orderId, AcceptOrderRequest request);
    Task<OrderDto> RejectOrderAsync(Guid supplierProfileId, Guid orderId, RejectOrderRequest request);
    Task<OrderDto> DispatchOrderAsync(Guid supplierProfileId, Guid orderId, DispatchOrderRequest request);
    Task<OrderDto> ConfirmDeliveryAsync(Guid restaurantProfileId, Guid orderId);
}
