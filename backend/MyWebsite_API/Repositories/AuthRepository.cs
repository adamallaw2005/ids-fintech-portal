using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class AuthRepository(IDbConnectionFactory connectionFactory) : IAuthRepository
{
    public async Task<UserAccount?> GetByEmailAsync(string email)
    {
        const string sql = """
            SELECT
                u.UserId,
                u.FullName,
                u.Email,
                u.PasswordHash,
                u.RoleId,
                r.RoleName,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt
            FROM Users u
            INNER JOIN Roles r ON r.RoleId = u.RoleId
            WHERE u.Email = @Email;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserAccount>(sql, new { Email = email });
    }

    public async Task<UserAccount?> GetByIdAsync(int userId)
    {
        const string sql = """
            SELECT
                u.UserId,
                u.FullName,
                u.Email,
                u.PasswordHash,
                u.RoleId,
                r.RoleName,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt
            FROM Users u
            INNER JOIN Roles r ON r.RoleId = u.RoleId
            WHERE u.UserId = @UserId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserAccount>(sql, new { UserId = userId });
    }

    public async Task<AdminSecuritySettings?> GetAdminSecuritySettingsAsync()
    {
        const string sql = """
            SELECT AdminSecuritySettingsId, AdminPromotionPasswordHash, IsConfigured
            FROM AdminSecuritySettings
            WHERE AdminSecuritySettingsId = 1;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<AdminSecuritySettings>(sql);
    }

    public async Task<bool> UpdateUserPasswordHashAsync(int userId, string passwordHash)
    {
        const string sql = """
            UPDATE Users
            SET PasswordHash = @PasswordHash,
                UpdatedAt = SYSUTCDATETIME()
            WHERE UserId = @UserId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { UserId = userId, PasswordHash = passwordHash }) > 0;
    }

    public async Task<bool> SetAdminPromotionPasswordHashAsync(string passwordHash)
    {
        const string sql = """
            UPDATE AdminSecuritySettings
            SET AdminPromotionPasswordHash = @PasswordHash,
                IsConfigured = 1,
                UpdatedAt = SYSUTCDATETIME()
            WHERE AdminSecuritySettingsId = 1
              AND IsConfigured = 0;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { PasswordHash = passwordHash }) > 0;
    }

    public async Task<bool> ChangeAdminPromotionPasswordHashAsync(string passwordHash)
    {
        const string sql = """
            UPDATE AdminSecuritySettings
            SET AdminPromotionPasswordHash = @PasswordHash,
                IsConfigured = 1,
                UpdatedAt = SYSUTCDATETIME()
            WHERE AdminSecuritySettingsId = 1
              AND IsConfigured = 1;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { PasswordHash = passwordHash }) > 0;
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, string roleName)
    {
        const string sql = """
            UPDATE u
            SET RoleId = r.RoleId,
                UpdatedAt = SYSUTCDATETIME()
            FROM Users u
            INNER JOIN Roles r ON r.RoleName = @RoleName
            WHERE u.UserId = @UserId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { UserId = userId, RoleName = roleName }) > 0;
    }
}
