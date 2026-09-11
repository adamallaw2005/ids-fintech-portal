using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class ClientRepository(IDbConnectionFactory connectionFactory) : IClientRepository
{
    public async Task<IReadOnlyList<Client>> GetAllAsync(string? search, string? country, string? status, int? productId)
    {
        const string sql = """
            SELECT DISTINCT c.ClientId, c.CompanyName, c.Country, c.ContactInformation,
                   c.Status, c.Notes, c.CreatedAt, c.UpdatedAt
            FROM Clients c
            LEFT JOIN Deployments d ON d.ClientId = c.ClientId
            WHERE (@Search IS NULL OR c.CompanyName LIKE '%' + @Search + '%')
              AND (@Country IS NULL OR c.Country = @Country)
              AND (@Status IS NULL OR c.Status = @Status)
              AND (@ProductId IS NULL OR d.ProductId = @ProductId)
            ORDER BY c.CompanyName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var clients = await connection.QueryAsync<Client>(sql, new
        {
            Search = search,
            Country = country,
            Status = status,
            ProductId = productId
        });

        return clients.AsList();
    }

    public async Task<Client?> GetByIdAsync(int clientId)
    {
        const string sql = """
            SELECT ClientId, CompanyName, Country, ContactInformation,
                   Status, Notes, CreatedAt, UpdatedAt
            FROM Clients
            WHERE ClientId = @ClientId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Client>(sql, new { ClientId = clientId });
    }

    public async Task<int> CreateAsync(ClientCreateRequest request)
    {
        const string sql = """
            INSERT INTO Clients (CompanyName, Country, ContactInformation, Status, Notes)
            OUTPUT INSERTED.ClientId
            VALUES (@CompanyName, @Country, @ContactInformation, @Status, @Notes);
            """;

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var id = await connection.ExecuteScalarAsync<int>(sql, request, transaction);
        const string assignmentSql = """
            INSERT INTO ClientResponsibilities (ClientId, TeamMemberId, ResponsibilityRole, Description)
            VALUES (@ClientId, @TeamMemberId, @ResponsibilityRole, @Description);
            """;
        if (request.Responsibilities.Count > 0)
        {
            await connection.ExecuteAsync(assignmentSql, request.Responsibilities.Select(assignment => new
            {
                ClientId = id,
                assignment.TeamMemberId,
                ResponsibilityRole = assignment.ResponsibilityRole.Trim(),
                assignment.Description
            }), transaction);
        }
        transaction.Commit();
        return id;
    }

    public async Task<bool> UpdateAsync(int clientId, ClientUpdateRequest request)
    {
        const string sql = """
            UPDATE Clients
            SET CompanyName = @CompanyName,
                Country = @Country,
                ContactInformation = @ContactInformation,
                Status = @Status,
                Notes = @Notes,
                UpdatedAt = SYSUTCDATETIME()
            WHERE ClientId = @ClientId;
            """;

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var updated = await connection.ExecuteAsync(sql, new
        {
            ClientId = clientId,
            request.CompanyName,
            request.Country,
            request.ContactInformation,
            request.Status,
            request.Notes
        }, transaction) > 0;
        if (!updated) return false;

        if (request.Responsibilities is not null)
        {
            await connection.ExecuteAsync(
                "DELETE FROM ClientResponsibilities WHERE ClientId = @Id;",
                new { Id = clientId }, transaction);
            const string assignmentSql = """
                INSERT INTO ClientResponsibilities (ClientId, TeamMemberId, ResponsibilityRole, Description)
                VALUES (@ClientId, @TeamMemberId, @ResponsibilityRole, @Description);
                """;
            if (request.Responsibilities.Count > 0)
            {
                await connection.ExecuteAsync(assignmentSql, request.Responsibilities.Select(assignment => new
                {
                    ClientId = clientId,
                    assignment.TeamMemberId,
                    ResponsibilityRole = assignment.ResponsibilityRole.Trim(),
                    assignment.Description
                }), transaction);
            }
        }
        transaction.Commit();
        return true;
    }

    public async Task<bool> DeleteAsync(int clientId)
    {
        const string sql = "DELETE FROM Clients WHERE ClientId = @ClientId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { ClientId = clientId }) > 0;
    }
}
