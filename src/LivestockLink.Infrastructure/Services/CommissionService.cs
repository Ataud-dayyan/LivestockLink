using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using LivestockLink.Core.Interfaces;
using LivestockLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LivestockLink.Infrastructure.Services;

public class CommissionService : ICommissionService
{
    private readonly ApplicationDbContext _context;
    public const string PlatformCommissionKey = "PlatformCommissionPercentage";
    public const decimal DefaultCommissionRate = 5.0m; // 5% default

    public CommissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CommissionRecord> CalculateAndRecordCommissionAsync(Guid orderId)
    {
        // 1. Prevent duplicate commission records
        var existing = await _context.CommissionRecords.FirstOrDefaultAsync(c => c.OrderId == orderId);
        if (existing != null)
        {
            return existing;
        }

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");
        }

        if (order.Status != OrderStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot calculate commission on order in status '{order.Status}'. Order must be 'Completed'.");
        }

        var rate = await GetCurrentCommissionRateAsync();
        var commissionAmount = Math.Round(order.TotalAmount * (rate / 100m), 2);
        var supplierNetPayout = order.TotalAmount - commissionAmount;

        var record = new CommissionRecord
        {
            OrderId = orderId,
            Order = order,
            TotalOrderValue = order.TotalAmount,
            CommissionPercentage = rate,
            CommissionAmount = commissionAmount,
            SupplierNetPayout = supplierNetPayout,
            CalculatedAt = DateTime.UtcNow,
            Status = "Recorded",
            Notes = $"Commission ({rate:F1}%) recorded for completed order {order.OrderNumber}"
        };

        _context.CommissionRecords.Add(record);
        await _context.SaveChangesAsync();

        return record;
    }

    public async Task<CommissionRecordDto?> GetCommissionByOrderIdAsync(Guid orderId)
    {
        var record = await _context.CommissionRecords.FirstOrDefaultAsync(c => c.OrderId == orderId);
        return record != null ? MapToDto(record) : null;
    }

    public async Task<IEnumerable<CommissionRecordDto>> GetAllCommissionsAsync()
    {
        var list = await _context.CommissionRecords
            .Include(c => c.Order)
            .OrderByDescending(c => c.CalculatedAt)
            .ToListAsync();

        return list.Select(MapToDto);
    }

    public async Task<decimal> GetCurrentCommissionRateAsync()
    {
        var setting = await _context.PlatformSettings.FirstOrDefaultAsync(s => s.Key == PlatformCommissionKey);
        if (setting != null && decimal.TryParse(setting.Value, out var parsedRate))
        {
            return parsedRate;
        }

        return DefaultCommissionRate;
    }

    public async Task<decimal> SetCurrentCommissionRateAsync(decimal newPercentage)
    {
        if (newPercentage < 0.1m || newPercentage > 50.0m)
        {
            throw new ArgumentOutOfRangeException(nameof(newPercentage), "Commission percentage must be between 0.1% and 50%.");
        }

        var setting = await _context.PlatformSettings.FirstOrDefaultAsync(s => s.Key == PlatformCommissionKey);
        if (setting == null)
        {
            setting = new PlatformSetting
            {
                Key = PlatformCommissionKey,
                Value = newPercentage.ToString("F2"),
                Description = "Default platform commission percentage charged on completed livestock transactions",
                UpdatedAt = DateTime.UtcNow
            };
            _context.PlatformSettings.Add(setting);
        }
        else
        {
            setting.Value = newPercentage.ToString("F2");
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return newPercentage;
    }

    private static CommissionRecordDto MapToDto(CommissionRecord c)
    {
        return new CommissionRecordDto
        {
            Id = c.Id,
            OrderId = c.OrderId,
            TotalOrderValue = c.TotalOrderValue,
            CommissionPercentage = c.CommissionPercentage,
            CommissionAmount = c.CommissionAmount,
            SupplierNetPayout = c.SupplierNetPayout,
            CalculatedAt = c.CalculatedAt,
            Status = c.Status,
            Notes = c.Notes
        };
    }
}
