using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface IUserManagementRepository
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(string? search, string? roleName, bool? isActive);
    Task<int?> CreateAsync(CreateUserRequest request, string passwordHash);
    Task<bool> UpdateAsync(int userId, UpdateUserRequest request, string? passwordHash);
    Task<bool> UpdateRoleAsync(int userId, string roleName);
    Task<bool> UpdateStatusAsync(int userId, bool isActive);
}
