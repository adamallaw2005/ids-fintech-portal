using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class ClientResponsibilityRepository(IDbConnectionFactory connectionFactory) : IClientResponsibilityRepository
{
    public async Task<IReadOnlyList<ClientResponsibilityDetails>> GetAllAsync(int? clientId, int? teamMemberId)
    {
        const string sql = """
            SELECT
                cr.ClientResponsibilityId,
                cr.ClientId,
                c.CompanyName,
                cr.TeamMemberId,
                tm.FullName AS TeamMemberName,
                cr.ResponsibilityRole,
                cr.Description,
                cr.CreatedAt
            FROM ClientResponsibilities cr
            INNER JOIN Clients c ON c.ClientId = cr.ClientId
            INNER JOIN TeamMembers tm ON tm.TeamMemberId = cr.TeamMemberId
            WHERE (@ClientId IS NULL OR cr.ClientId = @ClientId)
              AND (@TeamMemberId IS NULL OR cr.TeamMemberId = @TeamMemberId)
            ORDER BY c.CompanyName, tm.FullName, cr.ResponsibilityRole;
            """;

        using var connection = connectionFactory.CreateConnection();
        var responsibilities = await connection.QueryAsync<ClientResponsibilityDetails>(sql, new
        {
            ClientId = clientId,
            TeamMemberId = teamMemberId
        });

        return responsibilities.AsList();
    }

    public async Task<ClientResponsibilityDetails?> GetByIdAsync(int responsibilityId)
    {
        const string sql = """
            SELECT
                cr.ClientResponsibilityId,
                cr.ClientId,
                c.CompanyName,
                cr.TeamMemberId,
                tm.FullName AS TeamMemberName,
                cr.ResponsibilityRole,
                cr.Description,
                cr.CreatedAt
            FROM ClientResponsibilities cr
            INNER JOIN Clients c ON c.ClientId = cr.ClientId
            INNER JOIN TeamMembers tm ON tm.TeamMemberId = cr.TeamMemberId
            WHERE cr.ClientResponsibilityId = @ResponsibilityId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ClientResponsibilityDetails>(sql, new { ResponsibilityId = responsibilityId });
    }

    public async Task<int> CreateAsync(ClientResponsibilityRequest request)
    {
        const string sql = """
            INSERT INTO ClientResponsibilities
            (
                ClientId,
                TeamMemberId,
                ResponsibilityRole,
                Description
            )
            OUTPUT INSERTED.ClientResponsibilityId
            VALUES
            (
                @ClientId,
                @TeamMemberId,
                @ResponsibilityRole,
                @Description
            );
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, request);
    }

    public async Task<bool> UpdateAsync(int responsibilityId, ClientResponsibilityRequest request)
    {
        const string sql = """
            UPDATE ClientResponsibilities
            SET ClientId = @ClientId,
                TeamMemberId = @TeamMemberId,
                ResponsibilityRole = @ResponsibilityRole,
                Description = @Description
            WHERE ClientResponsibilityId = @ResponsibilityId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            ResponsibilityId = responsibilityId,
            request.ClientId,
            request.TeamMemberId,
            request.ResponsibilityRole,
            request.Description
        }) > 0;
    }

    public async Task<bool> DeleteAsync(int responsibilityId)
    {
        const string sql = "DELETE FROM ClientResponsibilities WHERE ClientResponsibilityId = @ResponsibilityId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { ResponsibilityId = responsibilityId }) > 0;
    }
}
