using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using LivestockLink.Core.Interfaces;
using LivestockLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LivestockLink.Infrastructure.Services;

public class DisputeService : IDisputeService
{
    private readonly ApplicationDbContext _context;
    private readonly ICommissionService _commissionService;

    public DisputeService(ApplicationDbContext context, ICommissionService commissionService)
    {
        _context = context;
        _commissionService = commissionService;
    }

    public async Task<DisputeDto> RaiseDisputeAsync(Guid restaurantProfileId, Guid orderId, CreateDisputeRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Dispute)
            .Include(o => o.RestaurantProfile)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.RestaurantProfileId == restaurantProfileId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found or unauthorized.");
        }

        if (order.Status != OrderStatus.Dispatched)
        {
            throw new InvalidOperationException($"Cannot raise dispute for order in status '{order.Status}'. Order must be 'Dispatched'.");
        }

        if (order.Dispute != null)
        {
            throw new InvalidOperationException("A dispute has already been filed for this order.");
        }

        var dispute = new Dispute
        {
            OrderId = orderId,
            Order = order,
            RaisedByUserId = order.RestaurantProfile.UserId,
            Reason = request.Reason.Trim(),
            Description = request.Description.Trim(),
            Status = DisputeStatus.Pending,
            RaisedAt = DateTime.UtcNow
        };

        order.Status = OrderStatus.Disputed;
        _context.Disputes.Add(dispute);
        await _context.SaveChangesAsync();

        return MapToDto(dispute, order.OrderNumber);
    }

    public async Task<DisputeDto> ResolveDisputeAsync(Guid adminUserId, Guid disputeId, ResolveDisputeRequest request)
    {
        var dispute = await _context.Disputes
            .Include(d => d.Order)
                .ThenInclude(o => o.Product)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            throw new KeyNotFoundException("Dispute record not found.");
        }

        dispute.Status = request.Status;
        dispute.ResolutionNotes = request.ResolutionNotes.Trim();
        dispute.ResolvedByUserId = adminUserId;
        dispute.ResolvedAt = DateTime.UtcNow;

        if (request.Status == DisputeStatus.ResolvedCompleted)
        {
            dispute.Order.Status = OrderStatus.Completed;
            dispute.Order.CompletedAt = DateTime.UtcNow;
            await _commissionService.CalculateAndRecordCommissionAsync(dispute.Order.Id);
        }
        else if (request.Status == DisputeStatus.ResolvedRefunded)
        {
            dispute.Order.Status = OrderStatus.Cancelled;
            dispute.Order.CancelledAt = DateTime.UtcNow;
            dispute.Order.CancellationReason = $"Cancelled via dispute resolution: {request.ResolutionNotes}";
            // Return goods back to stock
            dispute.Order.Product.AvailableQuantity += dispute.Order.Quantity;
        }
        else if (request.Status == DisputeStatus.Rejected)
        {
            // Reset to Dispatched so customer can confirm delivery once clarified
            dispute.Order.Status = OrderStatus.Dispatched;
        }

        await _context.SaveChangesAsync();
        return MapToDto(dispute, dispute.Order.OrderNumber);
    }

    public async Task<IEnumerable<DisputeDto>> GetAllDisputesAsync()
    {
        var list = await _context.Disputes
            .Include(d => d.Order)
            .OrderByDescending(d => d.RaisedAt)
            .ToListAsync();

        return list.Select(d => MapToDto(d, d.Order.OrderNumber));
    }

    public async Task<DisputeDto?> GetDisputeByOrderIdAsync(Guid orderId)
    {
        var dispute = await _context.Disputes
            .Include(d => d.Order)
            .FirstOrDefaultAsync(d => d.OrderId == orderId);

        return dispute != null ? MapToDto(dispute, dispute.Order.OrderNumber) : null;
    }

    private static DisputeDto MapToDto(Dispute d, string orderNumber)
    {
        return new DisputeDto
        {
            Id = d.Id,
            OrderId = d.OrderId,
            OrderNumber = orderNumber,
            RaisedByUserId = d.RaisedByUserId,
            Reason = d.Reason,
            Description = d.Description,
            Status = d.Status,
            ResolutionNotes = d.ResolutionNotes,
            RaisedAt = d.RaisedAt,
            ResolvedAt = d.ResolvedAt
        };
    }
}
