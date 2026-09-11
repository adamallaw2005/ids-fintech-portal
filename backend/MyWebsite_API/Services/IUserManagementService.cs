using MyWebsite_API.Models;

namespace MyWebsite_API.Services;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(string? search, string? roleName, bool? isActive);
    Task<UserResponse?> CreateAsync(CreateUserRequest request);
    Task<bool> UpdateAsync(int userId, UpdateUserRequest request);
    Task<bool> UpdateRoleAsync(int userId, string roleName);
    Task<bool> UpdateStatusAsync(int userId, bool isActive);
}
