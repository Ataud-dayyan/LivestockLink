using FluentAssertions;
using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using LivestockLink.Infrastructure.Data;
using LivestockLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LivestockLink.Tests;

public class ProductValidationTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateOrder_BelowMinimumOrder_ThrowsArgumentException()
    {
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);
        var orderService = new OrderService(context, commissionService);

        var buyer = new ApplicationUser { Email = "b@test.ng", FullName = "B", PhoneNumber = "+2348000000000", Role = UserRole.Restaurant };
        var restaurant = new RestaurantProfile { User = buyer, RestaurantName = "R" };
        var supplier = new SupplierProfile { User = new ApplicationUser { Email = "s@test.ng", FullName = "S", PhoneNumber = "+2348000000001", Role = UserRole.Supplier }, BusinessName = "S Farm", IsApproved = true };
        var product = new Product
        {
            SupplierProfile = supplier,
            Title = "Broiler",
            PricePerUnit = 8000m,
            AvailableQuantity = 100,
            MinimumOrderQuantity = 20, // Min is 20
            Unit = "Bird",
            IsActive = true
        };

        context.Users.Add(buyer);
        context.RestaurantProfiles.Add(restaurant);
        context.SupplierProfiles.Add(supplier);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Attempt to order 5 birds (below 20)
        var act = async () => await orderService.CreateOrderAsync(restaurant.Id, new CreateOrderRequest
        {
            ProductId = product.Id,
            Quantity = 5,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(1),
            DeliveryAddress = "Address",
            DeliveryCity = "City",
            DeliveryState = "State"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*below the supplier's minimum order requirement of 20 Bird*");
    }

    [Fact]
    public async Task CreateOrder_ExceedingAvailableStock_ThrowsArgumentException()
    {
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);
        var orderService = new OrderService(context, commissionService);

        var buyer = new ApplicationUser { Email = "b2@test.ng", FullName = "B2", PhoneNumber = "+2348000000000", Role = UserRole.Restaurant };
        var restaurant = new RestaurantProfile { User = buyer, RestaurantName = "R2" };
        var supplier = new SupplierProfile { User = new ApplicationUser { Email = "s2@test.ng", FullName = "S2", PhoneNumber = "+2348000000001", Role = UserRole.Supplier }, BusinessName = "S2 Farm", IsApproved = true };
        var product = new Product
        {
            SupplierProfile = supplier,
            Title = "Broiler",
            PricePerUnit = 8000m,
            AvailableQuantity = 30, // Only 30 in stock
            MinimumOrderQuantity = 10,
            Unit = "Bird",
            IsActive = true
        };

        context.Users.Add(buyer);
        context.RestaurantProfiles.Add(restaurant);
        context.SupplierProfiles.Add(supplier);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Attempt to order 50 birds
        var act = async () => await orderService.CreateOrderAsync(restaurant.Id, new CreateOrderRequest
        {
            ProductId = product.Id,
            Quantity = 50,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(1),
            DeliveryAddress = "Address",
            DeliveryCity = "City",
            DeliveryState = "State"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*exceeds available stock*");
    }

    [Fact]
    public async Task CreateProduct_WithZeroOrNegativePrice_ThrowsArgumentException()
    {
        using var context = GetInMemoryDbContext();
        var productService = new ProductService(context);

        var supplier = new SupplierProfile { User = new ApplicationUser { Email = "s3@test.ng", FullName = "S3", PhoneNumber = "+2348000000001", Role = UserRole.Supplier }, BusinessName = "S3 Farm", IsApproved = true };
        context.SupplierProfiles.Add(supplier);
        await context.SaveChangesAsync();

        var request = new CreateProductRequest
        {
            Title = "Free Chicken",
            Category = ProductCategory.Poultry,
            ProductForm = ProductForm.LiveAnimal,
            Unit = "Bird",
            PricePerUnit = 0m, // Invalid
            AvailableQuantity = 50,
            MinimumOrderQuantity = 5,
            DeliveryCoverage = "Lagos"
        };

        var act = async () => await productService.CreateProductAsync(supplier.Id, request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Product price must be greater than zero*");
    }

    [Fact]
    public async Task CreateOrder_OnSuspendedSupplier_ThrowsInvalidOperationException()
    {
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);
        var orderService = new OrderService(context, commissionService);

        var buyer = new ApplicationUser { Email = "b4@test.ng", FullName = "B4", PhoneNumber = "+2348000000000", Role = UserRole.Restaurant };
        var restaurant = new RestaurantProfile { User = buyer, RestaurantName = "R4" };
        var supplier = new SupplierProfile { User = new ApplicationUser { Email = "s4@test.ng", FullName = "S4", PhoneNumber = "+2348000000001", Role = UserRole.Supplier }, BusinessName = "Suspended Farm", IsApproved = false }; // Suspended!
        var product = new Product
        {
            SupplierProfile = supplier,
            Title = "Broiler",
            PricePerUnit = 8000m,
            AvailableQuantity = 100,
            MinimumOrderQuantity = 10,
            Unit = "Bird",
            IsActive = true
        };

        context.Users.Add(buyer);
        context.RestaurantProfiles.Add(restaurant);
        context.SupplierProfiles.Add(supplier);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var act = async () => await orderService.CreateOrderAsync(restaurant.Id, new CreateOrderRequest
        {
            ProductId = product.Id,
            Quantity = 15,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(1),
            DeliveryAddress = "Address",
            DeliveryCity = "City",
            DeliveryState = "State"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*supplier is currently suspended*");
    }
}
