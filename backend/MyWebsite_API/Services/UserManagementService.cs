using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class UserManagementService(
    IUserManagementRepository userRepository,
    IPasswordHashService passwordHashService,
    IAuthRepository authRepository) : IUserManagementService
{
    public Task<IReadOnlyList<UserResponse>> GetAllAsync(string? search, string? roleName, bool? isActive) =>
        userRepository.GetAllAsync(search, roleName, isActive);

    public async Task<UserResponse?> CreateAsync(CreateUserRequest request)
    {
        var roleName = NormalizeRoleName(request.RoleName);
        if (roleName is null)
        {
            return null;
        }

        request.RoleName = roleName;
        var userId = await userRepository.CreateAsync(request, passwordHashService.HashPassword(request.Password));
        return userId is null ? null : (await authRepository.GetByIdAsync(userId.Value))?.ToResponse();
    }

    public Task<bool> UpdateAsync(int userId, UpdateUserRequest request)
    {
        request.FullName = request.FullName.Trim();
        request.Email = request.Email.Trim();
        var passwordHash = string.IsNullOrWhiteSpace(request.NewPassword)
            ? null
            : passwordHashService.HashPassword(request.NewPassword);

        return userRepository.UpdateAsync(userId, request, passwordHash);
    }

    public async Task<bool> UpdateRoleAsync(int userId, string roleName)
    {
        var normalizedRole = NormalizeRoleName(roleName);
        return normalizedRole is not null && await userRepository.UpdateRoleAsync(userId, normalizedRole);
    }

    public Task<bool> UpdateStatusAsync(int userId, bool isActive) =>
        userRepository.UpdateStatusAsync(userId, isActive);

    private static string? NormalizeRoleName(string roleName)
    {
        if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        if (roleName.Equals("User", StringComparison.OrdinalIgnoreCase))
        {
            return "User";
        }

        return null;
    }
}
