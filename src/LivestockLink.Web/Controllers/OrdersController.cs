using System.Security.Claims;
using LivestockLink.Core.DTOs;
using LivestockLink.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivestockLink.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Restaurant profile not associated with account.");
        }

        var order = await _orderService.CreateOrderAsync(profileId, request);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    [HttpGet("restaurant")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetRestaurantOrders()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Restaurant profile not associated with account.");
        }

        var orders = await _orderService.GetRestaurantOrdersAsync(profileId);
        return Ok(orders);
    }

    [HttpGet("supplier")]
    [Authorize(Roles = "Supplier")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetSupplierOrders()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Supplier profile not associated with account.");
        }

        var orders = await _orderService.GetSupplierOrdersAsync(profileId);
        return Ok(orders);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var userId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role") ?? string.Empty;

        var order = await _orderService.GetOrderByIdAsync(id, userId, role);
        if (order == null)
        {
            return NotFound("Order not found.");
        }

        return Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<OrderDto>> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Restaurant profile not associated with account.");
        }

        var order = await _orderService.CancelOrderAsync(profileId, id, request);
        return Ok(order);
    }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Supplier")]
    public async Task<ActionResult<OrderDto>> AcceptOrder(Guid id, [FromBody] AcceptOrderRequest request)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Supplier profile not associated with account.");
        }

        var order = await _orderService.AcceptOrderAsync(profileId, id, request);
        return Ok(order);
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Supplier")]
    public async Task<ActionResult<OrderDto>> RejectOrder(Guid id, [FromBody] RejectOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Supplier profile not associated with account.");
        }

        var order = await _orderService.RejectOrderAsync(profileId, id, request);
        return Ok(order);
    }

    [HttpPost("{id:guid}/dispatch")]
    [Authorize(Roles = "Supplier")]
    public async Task<ActionResult<OrderDto>> DispatchOrder(Guid id, [FromBody] DispatchOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Supplier profile not associated with account.");
        }

        var order = await _orderService.DispatchOrderAsync(profileId, id, request);
        return Ok(order);
    }

    [HttpPost("{id:guid}/confirm-delivery")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<OrderDto>> ConfirmDelivery(Guid id)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Restaurant profile not associated with account.");
        }

        var order = await _orderService.ConfirmDeliveryAsync(profileId, id);
        return Ok(order);
    }

    private Guid GetProfileId()
    {
        var profileIdClaim = User.FindFirstValue("profileId");
        return Guid.TryParse(profileIdClaim, out var pid) ? pid : Guid.Empty;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var uid) ? uid : Guid.Empty;
    }
}
