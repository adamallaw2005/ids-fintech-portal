using MyWebsite_API.Models;

namespace MyWebsite_API.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<bool> SetupAdminPromotionPasswordAsync(string password);
    Task<bool> ChangeAdminPromotionPasswordAsync(string currentPassword, string newPassword);
    Task<AuthResponse?> PromoteToAdminAsync(int userId, string adminPromotionPassword);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
