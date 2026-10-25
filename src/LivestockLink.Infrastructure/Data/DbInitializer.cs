using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace LivestockLink.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Ensure database tables exist
        await context.Database.EnsureCreatedAsync();

        // Check if already seeded
        if (await context.Users.AnyAsync())
        {
            return; // DB already initialized
        }

        // 1. Platform Settings
        var defaultCommissionSetting = new PlatformSetting
        {
            Key = "PlatformCommissionPercentage",
            Value = "5.0",
            Description = "Default platform commission percentage charged on completed livestock transactions",
            UpdatedAt = DateTime.UtcNow
        };
        context.PlatformSettings.Add(defaultCommissionSetting);

        // 2. Default Admin
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var adminUser = new ApplicationUser
        {
            Email = "admin@livestocklink.ng",
            PasswordHash = adminPasswordHash,
            FullName = "Platform Administrator [DEMO]",
            PhoneNumber = "+2348030000001",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);

        // 3. Suppliers
        // Supplier 1: Al-Barakah Poultry Farms (Ibadan)
        var supp1User = new ApplicationUser
        {
            Email = "albarakah@livestocklink.ng",
            PasswordHash = adminPasswordHash,
            FullName = "Alhaji Kareem Adeleke [DEMO]",
            PhoneNumber = "+2348021112233",
            Role = UserRole.Supplier,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };
        var supp1Profile = new SupplierProfile
        {
            UserId = supp1User.Id,
            User = supp1User,
            BusinessName = "Al-Barakah Poultry & Agro Farms [DEMO]",
            FarmLocation = "Moniya Agro Corridor, Ibadan",
            State = "Oyo State",
            DeliveryCoverage = "Lagos, Oyo, Ogun, Osun states",
            Description = "Commercial broiler producers, layer eggs, and day-old chicks suppliers with refrigerated logistics delivery across South-West Nigeria.",
            IsApproved = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };
        context.Users.Add(supp1User);
        context.SupplierProfiles.Add(supp1Profile);

        // Supplier 2: Niger Valley Catfish Hatchery & Smokehouse (Asaba)
        var supp2User = new ApplicationUser
        {
            Email = "nigervalley@livestocklink.ng",
            PasswordHash = adminPasswordHash,
            FullName = "Engr. Chukwudi Okafor [DEMO]",
            PhoneNumber = "+2348035556677",
            Role = UserRole.Supplier,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };
        var supp2Profile = new SupplierProfile
        {
            UserId = supp2User.Id,
            User = supp2User,
            BusinessName = "Niger Valley Aquaculture & Smokehouse [DEMO]",
            FarmLocation = "Anwai Road Waterfront, Asaba",
            State = "Delta State",
            DeliveryCoverage = "Delta, Edo, Anambra, Rivers, Abuja FCT, Lagos",
            Description = "High-density concrete pond catfish farm. Supplies live aerated table-size catfish (1.2kg - 2.5kg) and oven-dried vacuum-sealed smoked catfish for restaurant stews and pepper soup.",
            IsApproved = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };
        context.Users.Add(supp2User);
        context.SupplierProfiles.Add(supp2Profile);

        // Supplier 3: Plateau Prime Livestock & Meat Processors (Jos)
        var supp3User = new ApplicationUser
        {
            Email = "plateauprime@livestocklink.ng",
            PasswordHash = adminPasswordHash,
            FullName = "Malam Danladi Yakubu [DEMO]",
            PhoneNumber = "+2348098889900",
            Role = UserRole.Supplier,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-4)
        };
        var supp3Profile = new SupplierProfile
        {
            UserId = supp3User.Id,
            User = supp3User,
            BusinessName = "Plateau Prime Beef & Ram Processors [DEMO]",
            FarmLocation = "Bukuru Livestock Market Road, Jos",
            State = "Plateau State",
            DeliveryCoverage = "Abuja FCT, Plateau, Kaduna, Nasarawa, Lagos Mainland",
            Description = "Premium grass-fed Fulani cattle, Red Sokoto goats, and fattened Balami rams. Available live or dressed in cold-chain hygienic cuts.",
            IsApproved = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-4)
        };
        context.Users.Add(supp3User);
        context.SupplierProfiles.Add(supp3Profile);

        // 4. Restaurants
        // Restaurant 1: Mama Cass Eateries (Lagos)
        var rest1User = new ApplicationUser
        {
            Email = "mamacass@livestocklink.ng",
            PasswordHash = adminPasswordHash,
            FullName = "Mrs. Folashade Balogun [DEMO]",
            PhoneNumber = "+2348053334455",
            Role = UserRole.Restaurant,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };
        var rest1Profile = new RestaurantProfile
        {
            UserId = rest1User.Id,
            User = rest1User,
            RestaurantName = "Mama Cass Eateries & Banquets [DEMO]",
            ContactPerson = "Mrs. Folashade Balogun (Procurement Head)",
            BusinessAddress = "Plot 12 Allen Avenue, Ikeja",
            City = "Ikeja",
            State = "Lagos State",
            PrimaryRequirements = "Weekly supply of 200+ broiler birds, dressed beef cuts, and live catfish for soups and buffets.",
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };
        context.Users.Add(rest1User);
        context.RestaurantProfiles.Add(rest1Profile);

        // Restaurant 2: Calabar Kitchen & Grills (Abuja)
        var rest2User = new ApplicationUser
        {
            Email = "calabarkitchen@livestocklink.ng",
            PasswordHash = adminPasswordHash,
            FullName = "Chef Idongesit Bassey [DEMO]",
            PhoneNumber = "+2348074445566",
            Role = UserRole.Restaurant,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };
        var rest2Profile = new RestaurantProfile
        {
            UserId = rest2User.Id,
            User = rest2User,
            RestaurantName = "Calabar Heritage Grills & Lounge [DEMO]",
            ContactPerson = "Chef Idongesit Bassey",
            BusinessAddress = "Adetokunbo Ademola Crescent, Wuse 2",
            City = "Abuja",
            State = "FCT",
            PrimaryRequirements = "Fresh live catfish for fisherman soup and pepper soup, plus live goat meat for Isi Ewu and Asun.",
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };
        context.Users.Add(rest2User);
        context.RestaurantProfiles.Add(rest2Profile);

        // 5. Products
        var p1 = new Product
        {
            SupplierProfile = supp1Profile,
            Title = "Farm-Fresh Broiler Chicken (Live Birds)",
            Category = ProductCategory.Poultry,
            ProductForm = ProductForm.LiveAnimal,
            Unit = "Bird",
            PricePerUnit = 8000m,
            AvailableQuantity = 450,
            MinimumOrderQuantity = 20,
            DeliveryCoverage = "Lagos (Mainland & Island), Ibadan, Abeokuta",
            Description = "Healthy, well-fed 8-week broilers weighing 2.3kg - 2.6kg average. Fed on bio-secure feed without synthetic growth hormones. Crates supplied on delivery.",
            ImageUrl = "https://images.unsplash.com/photo-1548550023-2bdb3c5beed7?auto=format&fit=crop&w=600&q=80",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var p2 = new Product
        {
            SupplierProfile = supp1Profile,
            Title = "Dressed Whole Broiler (Blast-Frozen & Vacuum Packed)",
            Category = ProductCategory.Poultry,
            ProductForm = ProductForm.ProcessedMeat,
            Unit = "Bird",
            PricePerUnit = 8800m,
            AvailableQuantity = 300,
            MinimumOrderQuantity = 10,
            DeliveryCoverage = "Lagos State, Ibadan, Ogun State",
            Description = "Hygienically slaughtered, plucked, eviscerated, and blast-frozen whole chicken. Ready for restaurant prep and marination. Dispatched in refrigerated vans.",
            ImageUrl = "https://images.unsplash.com/photo-1587593810167-a84920ea0781?auto=format&fit=crop&w=600&q=80",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };

        var p3 = new Product
        {
            SupplierProfile = supp2Profile,
            Title = "Live Table-Size African Catfish (1.2kg - 1.5kg avg)",
            Category = ProductCategory.CatfishFish,
            ProductForm = ProductForm.LiveAnimal,
            Unit = "Kg",
            PricePerUnit = 3500m,
            AvailableQuantity = 800,
            MinimumOrderQuantity = 25,
            DeliveryCoverage = "Abuja FCT, Delta, Edo, Anambra, Lagos",
            Description = "Vigorous, pristine table-size catfish reared in borehole flow-through ponds. Delivered alive in oxygenated mobile water tanks directly to restaurant kitchen basins.",
            ImageUrl = "https://images.unsplash.com/photo-1534043464124-3be32fe00099?auto=format&fit=crop&w=600&q=80",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-18)
        };

        var p4 = new Product
        {
            SupplierProfile = supp2Profile,
            Title = "Oven-Dried & Smoked Spiced Catfish (Vacuum Sealed)",
            Category = ProductCategory.CatfishFish,
            ProductForm = ProductForm.SmokedDried,
            Unit = "Kg",
            PricePerUnit = 7500m,
            AvailableQuantity = 150,
            MinimumOrderQuantity = 5,
            DeliveryCoverage = "Nationwide (Courier & Interstate haulage)",
            Description = "Traditional hardwood-smoked catfish with zero sand, zero mold, and long ambient shelf life. Ideal for Calabar soup, egusi, and native delicacies.",
            ImageUrl = "https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?auto=format&fit=crop&w=600&q=80",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-12)
        };

        var p5 = new Product
        {
            SupplierProfile = supp3Profile,
            Title = "Premium Red Sokoto Goat (Live Heavy Buck)",
            Category = ProductCategory.GoatsSheep,
            ProductForm = ProductForm.LiveAnimal,
            Unit = "Head",
            PricePerUnit = 75000m,
            AvailableQuantity = 60,
            MinimumOrderQuantity = 2,
            DeliveryCoverage = "Abuja FCT, Jos, Kaduna, Lagos (Express truck)",
            Description = "Authentic northern Red Sokoto goats. Renowned for tender, flavorful meat with lean fat distribution. Perfect for Nigerian Asun, goat meat pepper soup, and Isi Ewu.",
            ImageUrl = "https://images.unsplash.com/photo-1524024973431-2ad916746881?auto=format&fit=crop&w=600&q=80",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-25)
        };

        var p6 = new Product
        {
            SupplierProfile = supp3Profile,
            Title = "Prime Boneless Beef Carcass Cuts (Cleaned & Packaged)",
            Category = ProductCategory.CattleBeef,
            ProductForm = ProductForm.PackagedCuts,
            Unit = "Kg",
            PricePerUnit = 5200m,
            AvailableQuantity = 500,
            MinimumOrderQuantity = 20,
            DeliveryCoverage = "Abuja FCT, Plateau, Lagos Mainland",
            Description = "Veterinary-inspected prime Fulani bull meat. De-boned, trimmed, and portioned into 5kg food-grade vacuum bags for restaurant beef stews, suya strips, and pies.",
            ImageUrl = "https://images.unsplash.com/photo-1607623814075-e51df1bdc82f?auto=format&fit=crop&w=600&q=80",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        context.Products.AddRange(p1, p2, p3, p4, p5, p6);

        // 6. Seed Realistic Orders Across Workflow States
        // Order 1: COMPLETED with Commission Record (Mama Cass bought 50 broilers from Al-Barakah)
        var order1 = new Order
        {
            OrderNumber = "LL-2026-DEMO-1001",
            RestaurantProfile = rest1Profile,
            Product = p1,
            Quantity = 50,
            UnitPriceAtOrder = 8000m,
            TotalAmount = 400000m,
            Status = OrderStatus.Completed,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(-5),
            DeliveryAddress = "Plot 12 Allen Avenue, Ikeja",
            DeliveryCity = "Ikeja",
            DeliveryState = "Lagos State",
            RestaurantNotes = "Morning delivery preferred before 9:00 AM for lunch buffet prep.",
            SupplierNotes = "50 live broiler birds dispatched in plastic crates.",
            DispatchNotes = "Loaded onto truck KJA-889-XA with cold ventilation.",
            CourierContact = "Driver: Sunday Taiwo (+2348029988776)",
            SubmittedAt = DateTime.UtcNow.AddDays(-7),
            AcceptedAt = DateTime.UtcNow.AddDays(-6),
            DispatchedAt = DateTime.UtcNow.AddDays(-5),
            CompletedAt = DateTime.UtcNow.AddDays(-5)
        };
        context.Orders.Add(order1);

        var commission1 = new CommissionRecord
        {
            OrderId = order1.Id,
            Order = order1,
            TotalOrderValue = 400000m,
            CommissionPercentage = 5.0m,
            CommissionAmount = 20000m, // 5% of ₦400,000 = ₦20,000
            SupplierNetPayout = 380000m, // ₦400,000 - ₦20,000 = ₦380,000
            CalculatedAt = DateTime.UtcNow.AddDays(-5),
            Status = "Recorded",
            Notes = "Automated brokerage commission on completed delivery LL-2026-DEMO-1001"
        };
        context.CommissionRecords.Add(commission1);

        // Order 2: DISPATCHED (En route - Calabar Kitchen ordered 40kg Live Catfish from Niger Valley)
        var order2 = new Order
        {
            OrderNumber = "LL-2026-DEMO-1002",
            RestaurantProfile = rest2Profile,
            Product = p3,
            Quantity = 40,
            UnitPriceAtOrder = 3500m,
            TotalAmount = 140000m,
            Status = OrderStatus.Dispatched,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(1),
            DeliveryAddress = "Adetokunbo Ademola Crescent, Wuse 2",
            DeliveryCity = "Abuja",
            DeliveryState = "FCT",
            RestaurantNotes = "Please ensure active aerator is running during interstate transit.",
            SupplierNotes = "Pond harvested and packed into mobile oxygenated barrels.",
            DispatchNotes = "Dispatched via Interstate Express Agro Van ABJ-554-YX.",
            CourierContact = "Driver: Emeka Obi (+2348031122334)",
            SubmittedAt = DateTime.UtcNow.AddDays(-2),
            AcceptedAt = DateTime.UtcNow.AddDays(-1),
            DispatchedAt = DateTime.UtcNow.AddHours(-6)
        };
        context.Orders.Add(order2);

        // Order 3: ACCEPTED (Mama Cass ordered 20kg Boneless Beef from Plateau Prime)
        var order3 = new Order
        {
            OrderNumber = "LL-2026-DEMO-1003",
            RestaurantProfile = rest1Profile,
            Product = p6,
            Quantity = 20,
            UnitPriceAtOrder = 5200m,
            TotalAmount = 104000m,
            Status = OrderStatus.Accepted,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(2),
            DeliveryAddress = "Plot 12 Allen Avenue, Ikeja",
            DeliveryCity = "Ikeja",
            DeliveryState = "Lagos State",
            RestaurantNotes = "Must be vacuum-sealed in 5kg parcels.",
            SupplierNotes = "Carcass inspected and vacuum packaging scheduled for dispatch tomorrow morning.",
            SubmittedAt = DateTime.UtcNow.AddHours(-18),
            AcceptedAt = DateTime.UtcNow.AddHours(-10)
        };
        context.Orders.Add(order3);

        // Order 4: SUBMITTED (Calabar Kitchen ordered 25 Dressed Broilers from Al-Barakah - Awaiting Supplier)
        var order4 = new Order
        {
            OrderNumber = "LL-2026-DEMO-1004",
            RestaurantProfile = rest2Profile,
            Product = p2,
            Quantity = 25,
            UnitPriceAtOrder = 8800m,
            TotalAmount = 220000m,
            Status = OrderStatus.Submitted,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(3),
            DeliveryAddress = "Adetokunbo Ademola Crescent, Wuse 2",
            DeliveryCity = "Abuja",
            DeliveryState = "FCT",
            RestaurantNotes = "Special weekend catering order.",
            SubmittedAt = DateTime.UtcNow.AddHours(-4)
        };
        context.Orders.Add(order4);

        // Order 5: DISPUTED (Demonstration dispute for Admin resolution testing)
        var order5 = new Order
        {
            OrderNumber = "LL-2026-DEMO-1005",
            RestaurantProfile = rest1Profile,
            Product = p4,
            Quantity = 10,
            UnitPriceAtOrder = 7500m,
            TotalAmount = 75000m,
            Status = OrderStatus.Disputed,
            PreferredDeliveryDate = DateTime.UtcNow.AddDays(-3),
            DeliveryAddress = "Plot 12 Allen Avenue, Ikeja",
            DeliveryCity = "Ikeja",
            DeliveryState = "Lagos State",
            RestaurantNotes = "Smoked fish for banquet soup.",
            DispatchNotes = "Dispatched via standard dispatch rider.",
            CourierContact = "Rider: Rasheed (+2348021234567)",
            SubmittedAt = DateTime.UtcNow.AddDays(-6),
            AcceptedAt = DateTime.UtcNow.AddDays(-5),
            DispatchedAt = DateTime.UtcNow.AddDays(-4)
        };
        context.Orders.Add(order5);

        var dispute1 = new Dispute
        {
            OrderId = order5.Id,
            Order = order5,
            RaisedByUserId = rest1User.Id,
            Reason = "Outer packaging torn during transit",
            Description = "3 out of 10 bags suffered moisture exposure due to damaged courier carton in rain.",
            Status = DisputeStatus.Pending,
            RaisedAt = DateTime.UtcNow.AddDays(-3)
        };
        context.Disputes.Add(dispute1);

        await context.SaveChangesAsync();
    }
}
