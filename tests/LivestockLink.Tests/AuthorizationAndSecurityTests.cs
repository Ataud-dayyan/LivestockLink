using FluentAssertions;
using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using LivestockLink.Infrastructure.Data;
using LivestockLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LivestockLink.Tests;

public class AuthorizationAndSecurityTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetOrderById_UnrelatedUser_ThrowsUnauthorizedAccessException()
    {
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);
        var orderService = new OrderService(context, commissionService);

        var buyer1 = new ApplicationUser { Email = "buyer1@test.ng", FullName = "Buyer 1", PhoneNumber = "+2348000000000", Role = UserRole.Restaurant };
        var restaurant1 = new RestaurantProfile { User = buyer1, RestaurantName = "R1" };

        var intruderBuyer = new ApplicationUser { Email = "intruder@test.ng", FullName = "Intruder Buyer", PhoneNumber = "+2348000000009", Role = UserRole.Restaurant };

        var supplier = new SupplierProfile { User = new ApplicationUser { Email = "s@test.ng", FullName = "S", PhoneNumber = "+2348000000001", Role = UserRole.Supplier }, BusinessName = "S Farm", IsApproved = true };
        var product = new Product
        {
            SupplierProfile = supplier,
            Title = "Broiler",
            PricePerUnit = 8000m,
            AvailableQuantity = 100,
            MinimumOrderQuantity = 5,
            Unit = "Bird",
            IsActive = true
        };

        context.Users.AddRange(buyer1, intruderBuyer);
        context.RestaurantProfiles.Add(restaurant1);
        context.SupplierProfiles.Add(supplier);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var order = await orderService.CreateOrderAsync(restaurant1.Id, new CreateOrderRequest
        {
            ProductId = product.Id,
            Quantity = 10,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(1),
            DeliveryAddress = "Lekki, Lagos",
            DeliveryCity = "Lagos",
            DeliveryState = "Lagos State"
        });

        // Intruder tries to read buyer 1's order
        var act = async () => await orderService.GetOrderByIdAsync(order.Id, intruderBuyer.Id, UserRole.Restaurant.ToString());

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*You are not authorized to view this order*");
    }

    [Fact]
    public async Task GetOrderById_Admin_CanViewAnyOrder()
    {
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);
        var orderService = new OrderService(context, commissionService);

        var buyer = new ApplicationUser { Email = "buyer@test.ng", FullName = "Buyer", PhoneNumber = "+2348000000000", Role = UserRole.Restaurant };
        var restaurant = new RestaurantProfile { User = buyer, RestaurantName = "R" };
        var supplier = new SupplierProfile { User = new ApplicationUser { Email = "s@test.ng", FullName = "S", PhoneNumber = "+2348000000001", Role = UserRole.Supplier }, BusinessName = "S Farm", IsApproved = true };
        var product = new Product
        {
            SupplierProfile = supplier,
            Title = "Broiler",
            PricePerUnit = 8000m,
            AvailableQuantity = 100,
            MinimumOrderQuantity = 5,
            Unit = "Bird",
            IsActive = true
        };

        var adminUser = new ApplicationUser { Email = "admin@livestocklink.ng", FullName = "Admin", PhoneNumber = "+2348030000000", Role = UserRole.Admin };

        context.Users.AddRange(buyer, adminUser);
        context.RestaurantProfiles.Add(restaurant);
        context.SupplierProfiles.Add(supplier);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var order = await orderService.CreateOrderAsync(restaurant.Id, new CreateOrderRequest
        {
            ProductId = product.Id,
            Quantity = 10,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(1),
            DeliveryAddress = "Lekki, Lagos",
            DeliveryCity = "Lagos",
            DeliveryState = "Lagos State"
        });

        // Admin views order
        var viewed = await orderService.GetOrderByIdAsync(order.Id, adminUser.Id, UserRole.Admin.ToString());
        viewed.Should().NotBeNull();
        viewed!.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task Login_DeactivatedUser_ThrowsUnauthorizedAccessException()
    {
        using var context = GetInMemoryDbContext();
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var authService = new AuthService(context, config);

        var user = new ApplicationUser
        {
            Email = "banned@test.ng",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FullName = "Banned User",
            PhoneNumber = "+2348000000000",
            Role = UserRole.Restaurant,
            IsActive = false // Deactivated!
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var act = async () => await authService.LoginAsync(new LoginRequest { Email = "banned@test.ng", Password = "Password123!" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*deactivated*");
    }
}
