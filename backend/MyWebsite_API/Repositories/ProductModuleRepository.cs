using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class ProductModuleRepository(IDbConnectionFactory connectionFactory) : IProductModuleRepository
{
    public async Task<IReadOnlyList<ProductModuleDetails>> GetAllAsync(int? productId, string? status)
    {
        const string sql = """
            SELECT
                pm.ModuleId,
                pm.ProductId,
                p.ProductName,
                pm.ModuleName,
                pm.Description,
                pm.Status,
                pm.CreatedAt,
                pm.UpdatedAt
            FROM ProductModules pm
            INNER JOIN Products p ON p.ProductId = pm.ProductId
            WHERE (@ProductId IS NULL OR pm.ProductId = @ProductId)
              AND (@Status IS NULL OR pm.Status = @Status)
            ORDER BY p.ProductName, pm.ModuleName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var modules = await connection.QueryAsync<ProductModuleDetails>(sql, new
        {
            ProductId = productId,
            Status = status
        });

        return modules.AsList();
    }

    public async Task<ProductModuleDetails?> GetByIdAsync(int moduleId)
    {
        const string sql = """
            SELECT
                pm.ModuleId,
                pm.ProductId,
                p.ProductName,
                pm.ModuleName,
                pm.Description,
                pm.Status,
                pm.CreatedAt,
                pm.UpdatedAt
            FROM ProductModules pm
            INNER JOIN Products p ON p.ProductId = pm.ProductId
            WHERE pm.ModuleId = @ModuleId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProductModuleDetails>(sql, new { ModuleId = moduleId });
    }

    public async Task<int> CreateAsync(ProductModuleRequest request)
    {
        const string sql = """
            INSERT INTO ProductModules
            (
                ProductId,
                ModuleName,
                Description,
                Status
            )
            OUTPUT INSERTED.ModuleId
            VALUES
            (
                @ProductId,
                @ModuleName,
                @Description,
                @Status
            );
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, request);
    }

    public async Task<bool> UpdateAsync(int moduleId, ProductModuleRequest request)
    {
        const string sql = """
            UPDATE ProductModules
            SET ProductId = @ProductId,
                ModuleName = @ModuleName,
                Description = @Description,
                Status = @Status,
                UpdatedAt = SYSUTCDATETIME()
            WHERE ModuleId = @ModuleId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            ModuleId = moduleId,
            request.ProductId,
            request.ModuleName,
            request.Description,
            request.Status
        }) > 0;
    }

    public async Task<bool> DeleteAsync(int moduleId)
    {
        const string sql = "DELETE FROM ProductModules WHERE ModuleId = @ModuleId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { ModuleId = moduleId }) > 0;
    }
}
