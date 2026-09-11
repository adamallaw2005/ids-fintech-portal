using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class DeploymentRepository(IDbConnectionFactory connectionFactory) : IDeploymentRepository
{
    public async Task<IReadOnlyList<DeploymentSummary>> GetAllAsync(
        int? clientId,
        int? productId,
        string? version,
        string? status,
        string? environment)
    {
        const string sql = """
            SELECT
                d.DeploymentId,
                d.ClientId,
                c.CompanyName AS ClientName,
                d.ProductId,
                p.ProductName,
                d.ProductVersion,
                d.GoLiveDate,
                d.DeploymentStatus,
                d.SupportTier,
                d.ClientSpecificNotes,
                COUNT(e.EnvironmentId) AS EnvironmentCount,
                d.CreatedAt,
                d.UpdatedAt
            FROM Deployments d
            INNER JOIN Clients c ON c.ClientId = d.ClientId
            INNER JOIN Products p ON p.ProductId = d.ProductId
            LEFT JOIN Environments e ON e.DeploymentId = d.DeploymentId
            WHERE (@ClientId IS NULL OR d.ClientId = @ClientId)
              AND (@ProductId IS NULL OR d.ProductId = @ProductId)
              AND (@Version IS NULL OR d.ProductVersion LIKE '%' + @Version + '%')
              AND (@Status IS NULL OR d.DeploymentStatus = @Status)
              AND (
                    @Environment IS NULL
                    OR EXISTS
                    (
                        SELECT 1
                        FROM Environments environmentFilter
                        WHERE environmentFilter.DeploymentId = d.DeploymentId
                          AND (
                                environmentFilter.EnvironmentName LIKE '%' + @Environment + '%'
                                OR environmentFilter.EnvironmentType = @Environment
                              )
                    )
                  )
            GROUP BY
                d.DeploymentId,
                d.ClientId,
                c.CompanyName,
                d.ProductId,
                p.ProductName,
                d.ProductVersion,
                d.GoLiveDate,
                d.DeploymentStatus,
                d.SupportTier,
                d.ClientSpecificNotes,
                d.CreatedAt,
                d.UpdatedAt
            ORDER BY d.CreatedAt DESC, c.CompanyName, p.ProductName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var deployments = await connection.QueryAsync<DeploymentSummary>(sql, new
        {
            ClientId = clientId,
            ProductId = productId,
            Version = version,
            Status = status,
            Environment = environment
        });

        return deployments.AsList();
    }

    public async Task<DeploymentDetails?> GetByIdAsync(int deploymentId)
    {
        const string sql = """
            SELECT
                d.DeploymentId,
                d.ClientId,
                c.CompanyName AS ClientName,
                d.ProductId,
                p.ProductName,
                d.ProductVersion,
                d.GoLiveDate,
                d.DeploymentStatus,
                d.SupportTier,
                d.ClientSpecificNotes,
                d.CreatedAt,
                d.UpdatedAt
            FROM Deployments d
            INNER JOIN Clients c ON c.ClientId = d.ClientId
            INNER JOIN Products p ON p.ProductId = d.ProductId
            WHERE d.DeploymentId = @DeploymentId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DeploymentDetails>(sql, new { DeploymentId = deploymentId });
    }

    public async Task<int> CreateAsync(DeploymentRequest request)
    {
        const string sql = """
            INSERT INTO Deployments
            (
                ClientId,
                ProductId,
                ProductVersion,
                GoLiveDate,
                DeploymentStatus,
                SupportTier,
                ClientSpecificNotes
            )
            OUTPUT INSERTED.DeploymentId
            VALUES
            (
                @ClientId,
                @ProductId,
                @ProductVersion,
                @GoLiveDate,
                @DeploymentStatus,
                @SupportTier,
                @ClientSpecificNotes
            );
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, request);
    }

    public async Task<bool> UpdateAsync(int deploymentId, DeploymentRequest request)
    {
        const string sql = """
            UPDATE Deployments
            SET ClientId = @ClientId,
                ProductId = @ProductId,
                ProductVersion = @ProductVersion,
                GoLiveDate = @GoLiveDate,
                DeploymentStatus = @DeploymentStatus,
                SupportTier = @SupportTier,
                ClientSpecificNotes = @ClientSpecificNotes,
                UpdatedAt = SYSUTCDATETIME()
            WHERE DeploymentId = @DeploymentId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            DeploymentId = deploymentId,
            request.ClientId,
            request.ProductId,
            request.ProductVersion,
            request.GoLiveDate,
            request.DeploymentStatus,
            request.SupportTier,
            request.ClientSpecificNotes
        }) > 0;
    }

    public async Task<bool> DeleteAsync(int deploymentId)
    {
        const string sql = "DELETE FROM Deployments WHERE DeploymentId = @DeploymentId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { DeploymentId = deploymentId }) > 0;
    }
}
