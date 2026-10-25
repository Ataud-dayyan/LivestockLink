namespace LivestockLink.Core.Entities;

public class PlatformSetting
{
    public string Key { get; set; } = string.Empty; // e.g. "PlatformCommissionPercentage"
    public string Value { get; set; } = string.Empty; // e.g. "5.0"
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
