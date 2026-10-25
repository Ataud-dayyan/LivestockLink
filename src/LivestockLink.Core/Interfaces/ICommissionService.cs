using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;

namespace LivestockLink.Core.Interfaces;

public interface ICommissionService
{
    Task<CommissionRecord> CalculateAndRecordCommissionAsync(Guid orderId);
    Task<CommissionRecordDto?> GetCommissionByOrderIdAsync(Guid orderId);
    Task<IEnumerable<CommissionRecordDto>> GetAllCommissionsAsync();
    Task<decimal> GetCurrentCommissionRateAsync();
    Task<decimal> SetCurrentCommissionRateAsync(decimal newPercentage);
}
