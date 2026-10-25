using System.Text;
using System.Text.Json.Serialization;
using LivestockLink.Core.Interfaces;
using LivestockLink.Infrastructure.Data;
using LivestockLink.Infrastructure.Services;
using LivestockLink.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Enable legacy timestamp behavior for Npgsql to seamlessly handle DateTime without UTC kind errors
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Suppress 'Server: Kestrel' response header to prevent server version disclosure
// (ZAP: "Timestamp Disclosure" / server fingerprinting mitigation)
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// 1. Controller configuration with String Enum Converter
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 2. Database Context with PostgreSQL (Npgsql)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=livestocklink_db;Username=postgres;Password=Dayyan1907;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("LivestockLink.Infrastructure")));

// 3. Application Services Registration
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICommissionService, CommissionService>();
builder.Services.AddScoped<IDisputeService, DisputeService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// 4. JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "LivestockLinkNigeriaSuperSecretKey2026AgriculturalBrokerageSecurityKeyMustBeVeryLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LivestockLink";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LivestockLinkUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. Swagger / OpenAPI Documentation with JWT Bearer Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LivestockLink Nigeria API",
        Version = "v1",
        Description = "B2B Livestock Supply Brokerage Platform connecting Nigerian restaurants with verified livestock, poultry, and fish suppliers.",
        Contact = new OpenApiContact
        {
            Name = "LivestockLink Nigeria Support",
            Email = "support@livestocklink.ng"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 6. Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 7. Custom Security Headers Middleware (OWASP ZAP Compliance)
app.Use(async (context, next) =>
{
    // Fix: "Missing Anti-clickjacking Header"
    context.Response.Headers["X-Frame-Options"] = "DENY";

    // Fix: Missing "X-Content-Type-Options: nosniff"
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // Fix: Missing Content-Security-Policy (CSP)
    // Note: 'unsafe-inline' for script-src is required because index.html contains inline
    // <script> blocks and app.js uses innerHTML with inline event handlers (onclick, onerror).
    // A nonce-based strict CSP would require a full frontend refactor; this is MVP-appropriate.
    // Google Fonts CDN has been eliminated — font-src is now 'self' only (fonts self-hosted).
    context.Response.Headers["Content-Security-Policy"] = 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "font-src 'self'; " +
        "img-src 'self' data: https://images.unsplash.com; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "object-src 'none'; " +
        "base-uri 'self';";

    // Additional hardening headers
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";

    // Remove server-identifying headers that leak implementation details
    context.Response.Headers.Remove("X-Powered-By");

    await next();
});

// 8. Swagger Documentation UI
 app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LivestockLink API v1");
    c.RoutePrefix = "swagger";
});

// 9. Serve Frontend Static Files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// 10. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback to index.html for SPA routes if file not found
app.MapFallbackToFile("index.html");

// 11. Automatic Database Initialization & Seeding on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Applying migrations and initializing PostgreSQL database...");
        await DbInitializer.InitializeAsync(context);
        logger.LogInformation("Database initialization and demo seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the PostgreSQL database.");
    }
}

app.Run();
