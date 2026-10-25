using LivestockLink.Core.DTOs;
using LivestockLink.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivestockLink.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ICommissionService _commissionService;

    public AdminController(IAdminService adminService, ICommissionService commissionService)
    {
        _adminService = adminService;
        _commissionService = commissionService;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<AdminDashboardMetricsDto>> GetMetrics()
    {
        var metrics = await _adminService.GetDashboardMetricsAsync();
        return Ok(metrics);
    }

    [HttpGet("suppliers")]
    public async Task<ActionResult<IEnumerable<SupplierSummaryDto>>> GetSuppliers()
    {
        var suppliers = await _adminService.GetAllSuppliersAsync();
        return Ok(suppliers);
    }

    [HttpGet("restaurants")]
    public async Task<ActionResult<IEnumerable<RestaurantSummaryDto>>> GetRestaurants()
    {
        var restaurants = await _adminService.GetAllRestaurantsAsync();
        return Ok(restaurants);
    }

    [HttpPost("suppliers/{id:guid}/approval")]
    public async Task<ActionResult> SetSupplierApproval(Guid id, [FromBody] SupplierApprovalRequest request)
    {
        var result = await _adminService.SetSupplierApprovalAsync(id, request.IsApproved);
        if (!result) return NotFound("Supplier not found.");
        return Ok(new { supplierId = id, isApproved = request.IsApproved });
    }

    [HttpPost("users/{id:guid}/status")]
    public async Task<ActionResult> SetUserStatus(Guid id, [FromBody] UserStatusUpdateRequest request)
    {
        var result = await _adminService.SetUserActiveStatusAsync(id, request.IsActive);
        if (!result) return NotFound("User not found.");
        return Ok(new { userId = id, isActive = request.IsActive });
    }

    [HttpGet("commissions")]
    public async Task<ActionResult<IEnumerable<CommissionRecordDto>>> GetCommissions()
    {
        var commissions = await _commissionService.GetAllCommissionsAsync();
        return Ok(commissions);
    }

    [HttpGet("commission-rate")]
    public async Task<ActionResult> GetCommissionRate()
    {
        var rate = await _commissionService.GetCurrentCommissionRateAsync();
        return Ok(new { commissionPercentage = rate });
    }

    [HttpPut("commission-rate")]
    public async Task<ActionResult> UpdateCommissionRate([FromBody] UpdateCommissionRateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var newRate = await _commissionService.SetCurrentCommissionRateAsync(request.CommissionPercentage);
        return Ok(new { commissionPercentage = newRate, message = "Commission rate successfully updated." });
    }
}
