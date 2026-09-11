using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class TeamMemberRepository(IDbConnectionFactory connectionFactory) : ITeamMemberRepository
{
    public async Task<IReadOnlyList<TeamMemberSummary>> GetAllAsync(string? search, string? status)
    {
        const string sql = """
            SELECT
                tm.TeamMemberId,
                tm.FullName,
                tm.JobTitle,
                tm.DepartmentTeam,
                tm.Email,
                tm.Status,
                (
                    SELECT COUNT(DISTINCT pr.ProductId)
                    FROM ProductResponsibilities pr
                    WHERE pr.TeamMemberId = tm.TeamMemberId
                ) AS ProductResponsibilityCount,
                (
                    SELECT COUNT(DISTINCT cr.ClientId)
                    FROM ClientResponsibilities cr
                    WHERE cr.TeamMemberId = tm.TeamMemberId
                ) AS ClientResponsibilityCount,
                tm.CreatedAt,
                tm.UpdatedAt
            FROM TeamMembers tm
            WHERE (@Search IS NULL OR tm.FullName LIKE '%' + @Search + '%'
                   OR tm.Email LIKE '%' + @Search + '%')
              AND (@Status IS NULL OR tm.Status = @Status)
            ORDER BY tm.FullName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var teamMembers = await connection.QueryAsync<TeamMemberSummary>(sql, new
        {
            Search = search,
            Status = status
        });

        return teamMembers.AsList();
    }

    public async Task<TeamMember?> GetByIdAsync(int teamMemberId)
    {
        const string sql = """
            SELECT TeamMemberId, FullName, JobTitle, DepartmentTeam,
                   Email, Status, CreatedAt, UpdatedAt
            FROM TeamMembers
            WHERE TeamMemberId = @TeamMemberId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<TeamMember>(sql, new { TeamMemberId = teamMemberId });
    }

    public async Task<int> CreateAsync(TeamMemberRequest request)
    {
        const string sql = """
            INSERT INTO TeamMembers
            (
                FullName,
                JobTitle,
                DepartmentTeam,
                Email,
                Status
            )
            OUTPUT INSERTED.TeamMemberId
            VALUES
            (
                @FullName,
                @JobTitle,
                @DepartmentTeam,
                @Email,
                @Status
            );
            """;

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var id = await connection.ExecuteScalarAsync<int>(sql, new {
            request.FullName, request.JobTitle, request.DepartmentTeam, request.Email, request.Status
        }, transaction);
        await SaveAssignments(connection, transaction, id, request);
        transaction.Commit();
        return id;
    }

    public async Task<bool> UpdateAsync(int teamMemberId, TeamMemberRequest request)
    {
        const string sql = """
            UPDATE TeamMembers
            SET FullName = @FullName,
                JobTitle = @JobTitle,
                DepartmentTeam = @DepartmentTeam,
                Email = @Email,
                Status = @Status,
                UpdatedAt = SYSUTCDATETIME()
            WHERE TeamMemberId = @TeamMemberId;
            """;

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var updated = await connection.ExecuteAsync(sql, new
        {
            TeamMemberId = teamMemberId,
            request.FullName,
            request.JobTitle,
            request.DepartmentTeam,
            request.Email,
            request.Status
        }, transaction) > 0;
        if (!updated) return false;
        await SaveAssignments(connection, transaction, teamMemberId, request);
        transaction.Commit();
        return true;
    }

    public async Task<bool> DeleteAsync(int teamMemberId)
    {
        const string sql = """
            DELETE FROM ProductResponsibilities WHERE TeamMemberId = @TeamMemberId;
            DELETE FROM ClientResponsibilities WHERE TeamMemberId = @TeamMemberId;
            DELETE FROM TeamMembers WHERE TeamMemberId = @TeamMemberId;
            SELECT @@ROWCOUNT;
            """;

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var deleted = await connection.ExecuteScalarAsync<int>(sql, new { TeamMemberId = teamMemberId }, transaction) > 0;
        transaction.Commit();
        return deleted;
    }

    private static async Task SaveAssignments(System.Data.IDbConnection connection,
        System.Data.IDbTransaction transaction, int teamMemberId, TeamMemberRequest request)
    {
        foreach (var (assignments, table, target) in new[] {
            (request.ProductAssignments, "ProductResponsibilities", "ProductId"),
            (request.ClientAssignments, "ClientResponsibilities", "ClientId")
        })
        {
            if (assignments is null) continue;
            await connection.ExecuteAsync($"DELETE FROM {table} WHERE TeamMemberId = @TeamMemberId;",
                new { TeamMemberId = teamMemberId }, transaction);
            if (assignments.Count == 0) continue;
            await connection.ExecuteAsync($"""
                INSERT INTO {table} ({target}, TeamMemberId, ResponsibilityRole, Description)
                VALUES (@TargetId, @TeamMemberId, @ResponsibilityRole, @Description);
                """, assignments.Select(x => new {
                    x.TargetId, TeamMemberId = teamMemberId,
                    ResponsibilityRole = x.ResponsibilityRole.Trim(), x.Description
                }), transaction);
        }
    }
}
