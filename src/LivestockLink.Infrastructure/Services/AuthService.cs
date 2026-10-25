using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;
using LivestockLink.Core.Enums;
using LivestockLink.Core.Interfaces;
using LivestockLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LivestockLink.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower().Trim());
        if (existingUser)
        {
            throw new InvalidOperationException("An account with this email address already exists.");
        }

        var user = new ApplicationUser
        {
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        Guid profileId = Guid.Empty;
        string profileName = string.Empty;

        if (request.Role == UserRole.Restaurant)
        {
            var restProfile = new RestaurantProfile
            {
                UserId = user.Id,
                User = user,
                RestaurantName = !string.IsNullOrWhiteSpace(request.RestaurantName) ? request.RestaurantName.Trim() : request.FullName,
                ContactPerson = request.FullName.Trim(),
                BusinessAddress = request.BusinessAddress?.Trim() ?? string.Empty,
                City = request.City?.Trim() ?? "Lagos",
                State = request.State?.Trim() ?? "Lagos State",
                PrimaryRequirements = request.PrimaryRequirements?.Trim() ?? "General Livestock & Poultry",
                CreatedAt = DateTime.UtcNow
            };
            _context.RestaurantProfiles.Add(restProfile);
            profileId = restProfile.Id;
            profileName = restProfile.RestaurantName;
        }
        else if (request.Role == UserRole.Supplier)
        {
            var suppProfile = new SupplierProfile
            {
                UserId = user.Id,
                User = user,
                BusinessName = !string.IsNullOrWhiteSpace(request.SupplierBusinessName) ? request.SupplierBusinessName.Trim() : $"{request.FullName}'s Farm",
                FarmLocation = request.FarmLocation?.Trim() ?? "South West Agricultural Corridor",
                State = request.State?.Trim() ?? "Oyo State",
                DeliveryCoverage = request.DeliveryCoverage?.Trim() ?? "Lagos, Oyo, Ogun",
                Description = request.SupplierDescription?.Trim() ?? "Verified Livestock and Poultry producer",
                IsApproved = true, // Default active for seamless MVP evaluation
                CreatedAt = DateTime.UtcNow
            };
            _context.SupplierProfiles.Add(suppProfile);
            profileId = suppProfile.Id;
            profileName = suppProfile.BusinessName;
        }

        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user, profileId);

        return new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                ProfileId = profileId != Guid.Empty ? profileId : null,
                ProfileName = profileName,
                IsActive = user.IsActive
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.RestaurantProfile)
            .Include(u => u.SupplierProfile)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower().Trim());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("This account has been deactivated. Please contact platform administration.");
        }

        Guid? profileId = null;
        string? profileName = null;

        if (user.Role == UserRole.Restaurant && user.RestaurantProfile != null)
        {
            profileId = user.RestaurantProfile.Id;
            profileName = user.RestaurantProfile.RestaurantName;
        }
        else if (user.Role == UserRole.Supplier && user.SupplierProfile != null)
        {
            if (!user.SupplierProfile.IsApproved)
            {
                throw new UnauthorizedAccessException("Supplier account is currently under review or suspended by platform administration.");
            }
            profileId = user.SupplierProfile.Id;
            profileName = user.SupplierProfile.BusinessName;
        }

        var token = GenerateJwtToken(user, profileId);

        return new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                ProfileId = profileId,
                ProfileName = profileName,
                IsActive = user.IsActive
            }
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.RestaurantProfile)
            .Include(u => u.SupplierProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        Guid? profileId = user.Role == UserRole.Restaurant ? user.RestaurantProfile?.Id :
                          user.Role == UserRole.Supplier ? user.SupplierProfile?.Id : null;

        string? profileName = user.Role == UserRole.Restaurant ? user.RestaurantProfile?.RestaurantName :
                              user.Role == UserRole.Supplier ? user.SupplierProfile?.BusinessName : null;

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            ProfileId = profileId,
            ProfileName = profileName,
            IsActive = user.IsActive
        };
    }

    public async Task<bool> UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateJwtToken(ApplicationUser user, Guid? profileId)
    {
        var secretKey = _configuration["Jwt:Key"] ?? "LivestockLinkNigeriaSuperSecureKey2026!@#$%^&*()_+";
        var issuer = _configuration["Jwt:Issuer"] ?? "LivestockLink";
        var audience = _configuration["Jwt:Audience"] ?? "LivestockLinkUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role", user.Role.ToString())
        };

        if (profileId.HasValue && profileId.Value != Guid.Empty)
        {
            claims.Add(new Claim("profileId", profileId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
