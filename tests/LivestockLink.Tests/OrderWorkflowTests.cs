using FluentAssertions;
using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using LivestockLink.Infrastructure.Data;
using LivestockLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LivestockLink.Tests;

public class OrderWorkflowTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CompleteOrderWorkflow_FromSubmissionToDeliveryAndCommission_Succeeds()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);
        var orderService = new OrderService(context, commissionService);

        // Setup buyer and supplier
        var buyer = new ApplicationUser { Email = "buyer@test.ng", FullName = "Test Buyer", PhoneNumber = "+2348011111111", Role = UserRole.Restaurant };
        var restaurant = new RestaurantProfile { User = buyer, RestaurantName = "Lagos Kitchen", City = "Ikeja", State = "Lagos State" };

        var supplierUser = new ApplicationUser { Email = "supplier@test.ng", FullName = "Test Supplier", PhoneNumber = "+2348022222222", Role = UserRole.Supplier };
        var supplier = new SupplierProfile { User = supplierUser, BusinessName = "Ibadan Poultry Ltd", IsApproved = true };

        var product = new Product
        {
            SupplierProfile = supplier,
            Title = "Broiler Chicken",
            Category = ProductCategory.Poultry,
            ProductForm = ProductForm.LiveAnimal,
            Unit = "Bird",
            PricePerUnit = 8000m,
            AvailableQuantity = 100,
            MinimumOrderQuantity = 10,
            IsActive = true
        };

        context.Users.AddRange(buyer, supplierUser);
        context.RestaurantProfiles.Add(restaurant);
        context.SupplierProfiles.Add(supplier);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Step 1: Restaurant submits order for 50 birds @ ₦8,000 = ₦400,000
        var createRequest = new CreateOrderRequest
        {
            ProductId = product.Id,
            Quantity = 50,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(2),
            DeliveryAddress = "Plot 5 Admiralty Way, Lekki",
            DeliveryCity = "Lagos",
            DeliveryState = "Lagos State",
            RestaurantNotes = "Urgent weekend order"
        };

        var createdOrder = await orderService.CreateOrderAsync(restaurant.Id, createRequest);

        createdOrder.Should().NotBeNull();
        createdOrder.Status.Should().Be(OrderStatus.Submitted);
        createdOrder.TotalAmount.Should().Be(400000m);
        product.AvailableQuantity.Should().Be(50); // Stock reserved: 100 - 50 = 50

        // Step 2: Supplier accepts the order
        var acceptRequest = new AcceptOrderRequest { SupplierNotes = "Preparing cages for delivery" };
        var acceptedOrder = await orderService.AcceptOrderAsync(supplier.Id, createdOrder.Id, acceptRequest);
        acceptedOrder.Status.Should().Be(OrderStatus.Accepted);

        // Step 3: Supplier dispatches order
        var dispatchRequest = new DispatchOrderRequest
        {
            DispatchNotes = "Loaded in van KJA-101-XA",
            CourierContact = "Driver: John (+2348090001122)"
        };
        var dispatchedOrder = await orderService.DispatchOrderAsync(supplier.Id, createdOrder.Id, dispatchRequest);
        dispatchedOrder.Status.Should().Be(OrderStatus.Dispatched);

        // Step 4: Restaurant confirms delivery -> order completed & commission calculated
        var completedOrder = await orderService.ConfirmDeliveryAsync(restaurant.Id, createdOrder.Id);
        completedOrder.Status.Should().Be(OrderStatus.Completed);
        completedOrder.Commission.Should().NotBeNull();
        completedOrder.Commission!.TotalOrderValue.Should().Be(400000m);
        completedOrder.Commission.CommissionPercentage.Should().Be(5.0m);
        completedOrder.Commission.CommissionAmount.Should().Be(20000m); // 5% of ₦400,000
        completedOrder.Commission.SupplierNetPayout.Should().Be(380000m); // ₦400,000 - ₦20,000
    }

    [Fact]
    public async Task RestaurantCanCancel_SubmittedOrder_AndStockIsRestored()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);
        var orderService = new OrderService(context, commissionService);

        var buyer = new ApplicationUser { Email = "buyer2@test.ng", FullName = "Buyer 2", PhoneNumber = "+2348000000000", Role = UserRole.Restaurant };
        var restaurant = new RestaurantProfile { User = buyer, RestaurantName = "Abuja Grills" };
        var supplier = new SupplierProfile { User = new ApplicationUser { Email = "sup2@test.ng", FullName = "Sup 2", PhoneNumber = "+2348000000001", Role = UserRole.Supplier }, BusinessName = "Fish Farm", IsApproved = true };
        var product = new Product
        {
            SupplierProfile = supplier,
            Title = "Table Catfish",
            Category = ProductCategory.CatfishFish,
            PricePerUnit = 3000m,
            AvailableQuantity = 100,
            MinimumOrderQuantity = 5,
            IsActive = true
        };

        context.Users.Add(buyer);
        context.RestaurantProfiles.Add(restaurant);
        context.SupplierProfiles.Add(supplier);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var order = await orderService.CreateOrderAsync(restaurant.Id, new CreateOrderRequest
        {
            ProductId = product.Id,
            Quantity = 20,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(1),
            DeliveryAddress = "Wuse 2, Abuja",
            DeliveryCity = "Abuja",
            DeliveryState = "FCT"
        });

        product.AvailableQuantity.Should().Be(80);

        // Cancel order
        var cancelled = await orderService.CancelOrderAsync(restaurant.Id, order.Id, new CancelOrderRequest { Reason = "Menu changed" });

        cancelled.Status.Should().Be(OrderStatus.Cancelled);
        product.AvailableQuantity.Should().Be(100); // Stock restored!
    }

    [Fact]
    public async Task RestaurantCannotCancel_AcceptedOrDispatchedOrder()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var commissionService = new CommissionService(context);
        var orderService = new OrderService(context, commissionService);

        var buyer = new ApplicationUser { Email = "buyer3@test.ng", FullName = "Buyer 3", PhoneNumber = "+2348000000000", Role = UserRole.Restaurant };
        var restaurant = new RestaurantProfile { User = buyer, RestaurantName = "Abuja Grills" };
        var supplier = new SupplierProfile { User = new ApplicationUser { Email = "sup3@test.ng", FullName = "Sup 3", PhoneNumber = "+2348000000001", Role = UserRole.Supplier }, BusinessName = "Fish Farm", IsApproved = true };
        var product = new Product
        {
            SupplierProfile = supplier,
            Title = "Table Catfish",
            Category = ProductCategory.CatfishFish,
            PricePerUnit = 3000m,
            AvailableQuantity = 100,
            MinimumOrderQuantity = 5,
            IsActive = true
        };

        context.Users.Add(buyer);
        context.RestaurantProfiles.Add(restaurant);
        context.SupplierProfiles.Add(supplier);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var order = await orderService.CreateOrderAsync(restaurant.Id, new CreateOrderRequest
        {
            ProductId = product.Id,
            Quantity = 10,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(1),
            DeliveryAddress = "Wuse 2, Abuja",
            DeliveryCity = "Abuja",
            DeliveryState = "FCT"
        });

        // Supplier accepts
        await orderService.AcceptOrderAsync(supplier.Id, order.Id, new AcceptOrderRequest());

        // Restaurant attempt to cancel should throw
        var cancelAction = async () => await orderService.CancelOrderAsync(restaurant.Id, order.Id, new CancelOrderRequest { Reason = "Changed mind" });

        await cancelAction.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Orders can only be cancelled while in 'Submitted' status*");
    }
}
