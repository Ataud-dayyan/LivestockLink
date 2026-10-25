using System.ComponentModel.DataAnnotations;

namespace LivestockLink.Core.DTOs;

public class CommissionRecordDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal TotalOrderValue { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal SupplierNetPayout { get; set; }
    public DateTime CalculatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class PlatformSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class UpdateCommissionRateRequest
{
    [Required, Range(0.1, 50.0, ErrorMessage = "Commission percentage must be between 0.1% and 50%.")]
    public decimal CommissionPercentage { get; set; }
}
