using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class AuthService(
    IAuthRepository authRepository,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService) : IAuthService
{
    private const string InitialAdminEmail = "admin@idsfintech.com";
    private const string InitialAdminPassword = "12345678";
    private const string InitialPlaceholderHash = "CHANGE_THIS_HASH_IN_BACKEND";
    private const string DemoPlaceholderHash = "DEMO_ONLY_REPLACE_WHEN_JWT_IS_IMPLEMENTED";

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim();
        var user = await authRepository.GetByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordIsValid = passwordHashService.VerifyPassword(request.Password, user.PasswordHash);

        if (!passwordIsValid &&
            user.Email.Equals(InitialAdminEmail, StringComparison.OrdinalIgnoreCase) &&
            user.RoleName == "Admin" &&
            (user.PasswordHash == InitialPlaceholderHash || user.PasswordHash == DemoPlaceholderHash) &&
            request.Password == InitialAdminPassword)
        {
            var passwordHash = passwordHashService.HashPassword(InitialAdminPassword);
            await authRepository.UpdateUserPasswordHashAsync(user.UserId, passwordHash);
            user.PasswordHash = passwordHash;
            passwordIsValid = true;
        }

        return passwordIsValid ? jwtTokenService.CreateToken(user) : null;
    }

    public async Task<bool> SetupAdminPromotionPasswordAsync(string password)
    {
        var settings = await authRepository.GetAdminSecuritySettingsAsync();
        if (settings is null || settings.IsConfigured)
        {
            return false;
        }

        return await authRepository.SetAdminPromotionPasswordHashAsync(
            passwordHashService.HashPassword(password));
    }

    public async Task<bool> ChangeAdminPromotionPasswordAsync(string currentPassword, string newPassword)
    {
        var settings = await authRepository.GetAdminSecuritySettingsAsync();
        if (settings?.IsConfigured != true ||
            string.IsNullOrWhiteSpace(settings.AdminPromotionPasswordHash) ||
            !passwordHashService.VerifyPassword(currentPassword, settings.AdminPromotionPasswordHash))
        {
            return false;
        }

        return await authRepository.ChangeAdminPromotionPasswordHashAsync(
            passwordHashService.HashPassword(newPassword));
    }

    public async Task<AuthResponse?> PromoteToAdminAsync(int userId, string adminPromotionPassword)
    {
        var settings = await authRepository.GetAdminSecuritySettingsAsync();
        if (settings?.IsConfigured != true ||
            string.IsNullOrWhiteSpace(settings.AdminPromotionPasswordHash) ||
            !passwordHashService.VerifyPassword(adminPromotionPassword, settings.AdminPromotionPasswordHash))
        {
            return null;
        }

        var user = await authRepository.GetByIdAsync(userId);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        if (user.RoleName != "Admin")
        {
            await authRepository.UpdateUserRoleAsync(userId, "Admin");
            user.RoleName = "Admin";
        }

        return jwtTokenService.CreateToken(user);
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await authRepository.GetByIdAsync(userId);
        if (user is null || !user.IsActive ||
            !passwordHashService.VerifyPassword(currentPassword, user.PasswordHash))
        {
            return false;
        }

        return await authRepository.UpdateUserPasswordHashAsync(
            userId,
            passwordHashService.HashPassword(newPassword));
    }
}
