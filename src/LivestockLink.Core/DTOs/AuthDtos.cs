using System.ComponentModel.DataAnnotations;
using LivestockLink.Core.Enums;

namespace LivestockLink.Core.DTOs;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } // Restaurant or Supplier

    // Restaurant profile fields (if Role == Restaurant)
    public string? RestaurantName { get; set; }
    public string? BusinessAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PrimaryRequirements { get; set; }

    // Supplier profile fields (if Role == Supplier)
    public string? SupplierBusinessName { get; set; }
    public string? FarmLocation { get; set; }
    public string? DeliveryCoverage { get; set; }
    public string? SupplierDescription { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? ProfileId { get; set; }
    public string? ProfileName { get; set; }
    public bool IsActive { get; set; }
}
