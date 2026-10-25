using System.Security.Claims;
using LivestockLink.Core.DTOs;
using LivestockLink.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivestockLink.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _disputeService;

    public DisputesController(IDisputeService disputeService)
    {
        _disputeService = disputeService;
    }

    [HttpPost("{orderId:guid}")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<DisputeDto>> RaiseDispute(Guid orderId, [FromBody] CreateDisputeRequest request)
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

        var dispute = await _disputeService.RaiseDisputeAsync(profileId, orderId, request);
        return Ok(dispute);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<DisputeDto>>> GetAllDisputes()
    {
        var disputes = await _disputeService.GetAllDisputesAsync();
        return Ok(disputes);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<DisputeDto>> GetDisputeByOrderId(Guid orderId)
    {
        var dispute = await _disputeService.GetDisputeByOrderIdAsync(orderId);
        if (dispute == null)
        {
            return NotFound("No dispute found for this order.");
        }
        return Ok(dispute);
    }

    [HttpPost("{disputeId:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DisputeDto>> ResolveDispute(Guid disputeId, [FromBody] ResolveDisputeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminUserId = GetUserId();
        var resolved = await _disputeService.ResolveDisputeAsync(adminUserId, disputeId, request);
        return Ok(resolved);
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
