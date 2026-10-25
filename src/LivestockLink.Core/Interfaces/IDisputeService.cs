using LivestockLink.Core.DTOs;

namespace LivestockLink.Core.Interfaces;

public interface IDisputeService
{
    Task<DisputeDto> RaiseDisputeAsync(Guid restaurantProfileId, Guid orderId, CreateDisputeRequest request);
    Task<DisputeDto> ResolveDisputeAsync(Guid adminUserId, Guid disputeId, ResolveDisputeRequest request);
    Task<IEnumerable<DisputeDto>> GetAllDisputesAsync();
    Task<DisputeDto?> GetDisputeByOrderIdAsync(Guid orderId);
}
