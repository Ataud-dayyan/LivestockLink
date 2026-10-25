using FluentAssertions;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using LivestockLink.Infrastructure.Data;
using LivestockLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LivestockLink.Tests;

public class CommissionCalculationTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CalculateCommission_CalculatesAccurateBrokerageFeeAndSupplierPayout()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);

        var order = new Order
        {
            OrderNumber = "LL-TEST-100",
            TotalAmount = 400000m,
            Status = OrderStatus.Completed
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var record = await commissionService.CalculateAndRecordCommissionAsync(order.Id);

        // Assert
        record.Should().NotBeNull();
        record.TotalOrderValue.Should().Be(400000m);
        record.CommissionPercentage.Should().Be(5.0m);
        record.CommissionAmount.Should().Be(20000m); // 5% of ₦400,000 = ₦20,000
        record.SupplierNetPayout.Should().Be(380000m); // ₦400,000 - ₦20,000 = ₦380,000

        var count = await context.CommissionRecords.CountAsync(c => c.OrderId == order.Id);
        count.Should().Be(1);
    }

    [Fact]
    public async Task DuplicateCommissionPrevention_ReturnsExistingRecord_WithoutAddingDuplicate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);

        var order = new Order
        {
            OrderNumber = "LL-TEST-200",
            TotalAmount = 500000m,
            Status = OrderStatus.Completed
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // First calculation
        var record1 = await commissionService.CalculateAndRecordCommissionAsync(order.Id);

        // Second calculation (duplicate attempt)
        var record2 = await commissionService.CalculateAndRecordCommissionAsync(order.Id);

        // Assert
        record1.Id.Should().Be(record2.Id);
        var totalRecords = await context.CommissionRecords.CountAsync(c => c.OrderId == order.Id);
        totalRecords.Should().Be(1);
    }

    [Fact]
    public async Task CalculateCommission_ThrowsIfOrderNotCompleted()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);

        var order = new Order
        {
            OrderNumber = "LL-TEST-300",
            TotalAmount = 150000m,
            Status = OrderStatus.Dispatched // Not completed!
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var act = async () => await commissionService.CalculateAndRecordCommissionAsync(order.Id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Order must be 'Completed'*");
    }

    [Fact]
    public async Task SetCommissionRate_UpdatesPlatformRate_AndCalculatesAccurately()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);

        // Update rate to 7.5%
        await commissionService.SetCurrentCommissionRateAsync(7.5m);

        var currentRate = await commissionService.GetCurrentCommissionRateAsync();
        currentRate.Should().Be(7.5m);

        var order = new Order
        {
            OrderNumber = "LL-TEST-400",
            TotalAmount = 200000m,
            Status = OrderStatus.Completed
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var record = await commissionService.CalculateAndRecordCommissionAsync(order.Id);

        record.CommissionPercentage.Should().Be(7.5m);
        record.CommissionAmount.Should().Be(15000m); // 7.5% of 200,000 = 15,000
        record.SupplierNetPayout.Should().Be(185000m);
    }
}
