using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class SetupAdminPasswordRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class AdminPasswordChangeRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class AdminPromotionRequest
{
    [Required]
    public string AdminPromotionPassword { get; set; } = string.Empty;
}

public sealed class CreateUserRequest
{
    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string RoleName { get; set; } = "User";

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateUserRoleRequest
{
    [Required, StringLength(50)]
    public string RoleName { get; set; } = "User";
}

public sealed class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed class UpdateUserRequest
{
    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(200)]
    public string? NewPassword { get; set; }
}

public sealed class UserResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponse User { get; set; } = new();
}

public sealed class UserAccount
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public UserResponse ToResponse() => new()
    {
        UserId = UserId,
        FullName = FullName,
        Email = Email,
        RoleName = RoleName,
        IsActive = IsActive,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt
    };
}

public sealed class AdminSecuritySettings
{
    public byte AdminSecuritySettingsId { get; set; }
    public string? AdminPromotionPasswordHash { get; set; }
    public bool IsConfigured { get; set; }
}
