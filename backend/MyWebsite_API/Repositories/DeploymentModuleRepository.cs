using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class DeploymentModuleRepository(IDbConnectionFactory connectionFactory) : IDeploymentModuleRepository
{
    public async Task<IReadOnlyList<DeploymentModuleDetails>> GetAllAsync(int? deploymentId, int? productId)
    {
        const string sql = """
            SELECT
                dm.DeploymentModuleId,
                dm.DeploymentId,
                d.ClientId,
                c.CompanyName AS ClientName,
                d.ProductId,
                p.ProductName,
                d.ProductVersion,
                dm.ModuleId,
                pm.ModuleName,
                pm.Description AS ModuleDescription,
                pm.Status AS ModuleStatus
            FROM DeploymentModules dm
            INNER JOIN Deployments d ON d.DeploymentId = dm.DeploymentId
            INNER JOIN Clients c ON c.ClientId = d.ClientId
            INNER JOIN Products p ON p.ProductId = d.ProductId
            INNER JOIN ProductModules pm ON pm.ModuleId = dm.ModuleId
            WHERE (@DeploymentId IS NULL OR dm.DeploymentId = @DeploymentId)
              AND (@ProductId IS NULL OR d.ProductId = @ProductId)
            ORDER BY c.CompanyName, p.ProductName, d.ProductVersion, pm.ModuleName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var modules = await connection.QueryAsync<DeploymentModuleDetails>(sql, new
        {
            DeploymentId = deploymentId,
            ProductId = productId
        });

        return modules.AsList();
    }

    public async Task<DeploymentModuleDetails?> GetByIdAsync(int deploymentModuleId)
    {
        const string sql = """
            SELECT
                dm.DeploymentModuleId,
                dm.DeploymentId,
                d.ClientId,
                c.CompanyName AS ClientName,
                d.ProductId,
                p.ProductName,
                d.ProductVersion,
                dm.ModuleId,
                pm.ModuleName,
                pm.Description AS ModuleDescription,
                pm.Status AS ModuleStatus
            FROM DeploymentModules dm
            INNER JOIN Deployments d ON d.DeploymentId = dm.DeploymentId
            INNER JOIN Clients c ON c.ClientId = d.ClientId
            INNER JOIN Products p ON p.ProductId = d.ProductId
            INNER JOIN ProductModules pm ON pm.ModuleId = dm.ModuleId
            WHERE dm.DeploymentModuleId = @DeploymentModuleId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DeploymentModuleDetails>(sql, new { DeploymentModuleId = deploymentModuleId });
    }

    public async Task<int?> CreateAsync(DeploymentModuleRequest request)
    {
        const string sql = """
            DECLARE @ExistingId INT =
            (
                SELECT DeploymentModuleId
                FROM DeploymentModules
                WHERE DeploymentId = @DeploymentId
                  AND ModuleId = @ModuleId
            );

            IF @ExistingId IS NOT NULL
            BEGIN
                SELECT @ExistingId;
            END
            ELSE
            BEGIN
                INSERT INTO DeploymentModules (DeploymentId, ModuleId)
                OUTPUT INSERTED.DeploymentModuleId
                SELECT @DeploymentId, @ModuleId
                WHERE EXISTS
                (
                    SELECT 1
                    FROM Deployments d
                    INNER JOIN ProductModules pm ON pm.ProductId = d.ProductId
                    WHERE d.DeploymentId = @DeploymentId
                      AND pm.ModuleId = @ModuleId
                );
            END;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<int?>(sql, request);
    }

    public async Task<bool> UpdateAsync(int deploymentModuleId, DeploymentModuleRequest request)
    {
        const string sql = """
            UPDATE dm
            SET DeploymentId = @DeploymentId,
                ModuleId = @ModuleId
            FROM DeploymentModules dm
            WHERE dm.DeploymentModuleId = @DeploymentModuleId
              AND EXISTS
              (
                  SELECT 1
                  FROM Deployments d
                  INNER JOIN ProductModules pm ON pm.ProductId = d.ProductId
                  WHERE d.DeploymentId = @DeploymentId
                    AND pm.ModuleId = @ModuleId
              );
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            DeploymentModuleId = deploymentModuleId,
            request.DeploymentId,
            request.ModuleId
        }) > 0;
    }

    public async Task<bool> DeleteAsync(int deploymentModuleId)
    {
        const string sql = "DELETE FROM DeploymentModules WHERE DeploymentModuleId = @DeploymentModuleId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { DeploymentModuleId = deploymentModuleId }) > 0;
    }
}
