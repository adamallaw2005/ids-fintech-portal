using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class UserManagementRepository(IDbConnectionFactory connectionFactory) : IUserManagementRepository
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(string? search, string? roleName, bool? isActive)
    {
        const string sql = """
            SELECT
                u.UserId,
                u.FullName,
                u.Email,
                r.RoleName,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt
            FROM Users u
            INNER JOIN Roles r ON r.RoleId = u.RoleId
            WHERE (@Search IS NULL OR u.FullName LIKE '%' + @Search + '%'
                   OR u.Email LIKE '%' + @Search + '%')
              AND (@RoleName IS NULL OR r.RoleName = @RoleName)
              AND (@IsActive IS NULL OR u.IsActive = @IsActive)
            ORDER BY u.FullName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var users = await connection.QueryAsync<UserResponse>(sql, new
        {
            Search = search,
            RoleName = roleName,
            IsActive = isActive
        });

        return users.AsList();
    }

    public async Task<int?> CreateAsync(CreateUserRequest request, string passwordHash)
    {
        const string sql = """
            INSERT INTO Users
            (
                FullName,
                Email,
                PasswordHash,
                RoleId,
                IsActive
            )
            OUTPUT INSERTED.UserId
            SELECT
                @FullName,
                @Email,
                @PasswordHash,
                r.RoleId,
                @IsActive
            FROM Roles r
            WHERE r.RoleName = @RoleName;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<int?>(sql, new
        {
            request.FullName,
            request.Email,
            PasswordHash = passwordHash,
            request.RoleName,
            request.IsActive
        });
    }

    public async Task<bool> UpdateAsync(int userId, UpdateUserRequest request, string? passwordHash)
    {
        const string sql = """
            IF EXISTS
            (
                SELECT 1
                FROM Users
                WHERE Email = @Email
                  AND UserId <> @UserId
            )
            BEGIN
                SELECT CAST(0 AS bit);
                RETURN;
            END;

            UPDATE Users
            SET FullName = @FullName,
                Email = @Email,
                PasswordHash = COALESCE(@PasswordHash, PasswordHash),
                UpdatedAt = SYSUTCDATETIME()
            WHERE UserId = @UserId;

            SELECT CAST(CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END AS bit);
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<bool>(sql, new
        {
            UserId = userId,
            request.FullName,
            request.Email,
            PasswordHash = passwordHash
        });
    }

    public async Task<bool> UpdateRoleAsync(int userId, string roleName)
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

    public async Task<bool> UpdateStatusAsync(int userId, bool isActive)
    {
        const string sql = """
            UPDATE Users
            SET IsActive = @IsActive,
                UpdatedAt = SYSUTCDATETIME()
            WHERE UserId = @UserId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { UserId = userId, IsActive = isActive }) > 0;
    }
}
