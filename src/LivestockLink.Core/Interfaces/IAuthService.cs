using LivestockLink.Core.DTOs;
using LivestockLink.Core.Entities;

namespace LivestockLink.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<bool> UpdateUserStatusAsync(Guid userId, bool isActive);
}
