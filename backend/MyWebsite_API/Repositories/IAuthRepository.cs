using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface IAuthRepository
{
    Task<UserAccount?> GetByEmailAsync(string email);
    Task<UserAccount?> GetByIdAsync(int userId);
    Task<AdminSecuritySettings?> GetAdminSecuritySettingsAsync();
    Task<bool> UpdateUserPasswordHashAsync(int userId, string passwordHash);
    Task<bool> SetAdminPromotionPasswordHashAsync(string passwordHash);
    Task<bool> ChangeAdminPromotionPasswordHashAsync(string passwordHash);
    Task<bool> UpdateUserRoleAsync(int userId, string roleName);
}
