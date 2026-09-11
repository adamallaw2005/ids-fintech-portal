using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class EnvironmentRepository(IDbConnectionFactory connectionFactory) : IEnvironmentRepository
{
    public async Task<IReadOnlyList<EnvironmentDetails>> GetAllAsync(int? deploymentId, string? type)
    {
        const string sql = """
            SELECT
                e.EnvironmentId,
                e.DeploymentId,
                c.CompanyName AS ClientName,
                p.ProductName,
                d.ProductVersion,
                e.EnvironmentName,
                e.EnvironmentType,
                e.Purpose,
                e.ServerName,
                e.OperatingSystem,
                e.ApplicationUrl,
                e.DatabaseInformation,
                e.MonitoringLink,
                e.AccessInstructionsReference,
                e.Notes,
                e.CreatedAt,
                e.UpdatedAt
            FROM Environments e
            INNER JOIN Deployments d ON d.DeploymentId = e.DeploymentId
            INNER JOIN Clients c ON c.ClientId = d.ClientId
            INNER JOIN Products p ON p.ProductId = d.ProductId
            WHERE (@DeploymentId IS NULL OR e.DeploymentId = @DeploymentId)
              AND (@Type IS NULL OR e.EnvironmentType = @Type)
            ORDER BY c.CompanyName, p.ProductName, d.ProductVersion, e.EnvironmentType, e.EnvironmentName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var environments = await connection.QueryAsync<EnvironmentDetails>(sql, new
        {
            DeploymentId = deploymentId,
            Type = type
        });

        return environments.AsList();
    }

    public async Task<EnvironmentDetails?> GetByIdAsync(int environmentId)
    {
        const string sql = """
            SELECT
                e.EnvironmentId,
                e.DeploymentId,
                c.CompanyName AS ClientName,
                p.ProductName,
                d.ProductVersion,
                e.EnvironmentName,
                e.EnvironmentType,
                e.Purpose,
                e.ServerName,
                e.OperatingSystem,
                e.ApplicationUrl,
                e.DatabaseInformation,
                e.MonitoringLink,
                e.AccessInstructionsReference,
                e.Notes,
                e.CreatedAt,
                e.UpdatedAt
            FROM Environments e
            INNER JOIN Deployments d ON d.DeploymentId = e.DeploymentId
            INNER JOIN Clients c ON c.ClientId = d.ClientId
            INNER JOIN Products p ON p.ProductId = d.ProductId
            WHERE e.EnvironmentId = @EnvironmentId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<EnvironmentDetails>(sql, new { EnvironmentId = environmentId });
    }

    public async Task<int> CreateAsync(EnvironmentRequest request)
    {
        const string sql = """
            INSERT INTO Environments
            (
                DeploymentId,
                EnvironmentName,
                EnvironmentType,
                Purpose,
                ServerName,
                OperatingSystem,
                ApplicationUrl,
                DatabaseInformation,
                MonitoringLink,
                AccessInstructionsReference,
                Notes
            )
            OUTPUT INSERTED.EnvironmentId
            VALUES
            (
                @DeploymentId,
                @EnvironmentName,
                @EnvironmentType,
                @Purpose,
                @ServerName,
                @OperatingSystem,
                @ApplicationUrl,
                @DatabaseInformation,
                @MonitoringLink,
                @AccessInstructionsReference,
                @Notes
            );
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, request);
    }

    public async Task<bool> UpdateAsync(int environmentId, EnvironmentRequest request)
    {
        const string sql = """
            UPDATE Environments
            SET DeploymentId = @DeploymentId,
                EnvironmentName = @EnvironmentName,
                EnvironmentType = @EnvironmentType,
                Purpose = @Purpose,
                ServerName = @ServerName,
                OperatingSystem = @OperatingSystem,
                ApplicationUrl = @ApplicationUrl,
                DatabaseInformation = @DatabaseInformation,
                MonitoringLink = @MonitoringLink,
                AccessInstructionsReference = @AccessInstructionsReference,
                Notes = @Notes,
                UpdatedAt = SYSUTCDATETIME()
            WHERE EnvironmentId = @EnvironmentId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            EnvironmentId = environmentId,
            request.DeploymentId,
            request.EnvironmentName,
            request.EnvironmentType,
            request.Purpose,
            request.ServerName,
            request.OperatingSystem,
            request.ApplicationUrl,
            request.DatabaseInformation,
            request.MonitoringLink,
            request.AccessInstructionsReference,
            request.Notes
        }) > 0;
    }

    public async Task<bool> DeleteAsync(int environmentId)
    {
        const string sql = "DELETE FROM Environments WHERE EnvironmentId = @EnvironmentId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { EnvironmentId = environmentId }) > 0;
    }
}
