using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class ProductRepository(IDbConnectionFactory connectionFactory) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(string? search, string? status, string? technology)
    {
        const string sql = """
            SELECT ProductId, ProductName, Description, BusinessPurpose, LifecycleStatus,
                   CurrentVersion, SupportedMarkets, Criticality, Technologies, Notes,
                   CreatedAt, UpdatedAt
            FROM Products
            WHERE (@Search IS NULL OR ProductName LIKE '%' + @Search + '%')
              AND (@Status IS NULL OR LifecycleStatus = @Status)
              AND (@Technology IS NULL OR Technologies LIKE '%' + @Technology + '%')
            ORDER BY ProductName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var products = await connection.QueryAsync<Product>(sql, new { Search = search, Status = status, Technology = technology });
        return products.AsList();
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        const string sql = """
            SELECT ProductId, ProductName, Description, BusinessPurpose, LifecycleStatus,
                   CurrentVersion, SupportedMarkets, Criticality, Technologies, Notes,
                   CreatedAt, UpdatedAt
            FROM Products
            WHERE ProductId = @ProductId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { ProductId = productId });
    }

    public async Task<int> CreateAsync(ProductCreateRequest request)
    {
        const string sql = """
            INSERT INTO Products
                (ProductName, Description, BusinessPurpose, LifecycleStatus, CurrentVersion,
                 SupportedMarkets, Criticality, Technologies, Notes)
            OUTPUT INSERTED.ProductId
            VALUES
                (@ProductName, @Description, @BusinessPurpose, @LifecycleStatus, @CurrentVersion,
                 @SupportedMarkets, @Criticality, @Technologies, @Notes);
            """;

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var id = await connection.ExecuteScalarAsync<int>(sql, request, transaction);
        const string assignmentSql = """
            INSERT INTO ProductResponsibilities (ProductId, TeamMemberId, ResponsibilityRole, Description)
            VALUES (@ProductId, @TeamMemberId, @ResponsibilityRole, @Description);
            """;
        if (request.Responsibilities.Count > 0)
        {
            await connection.ExecuteAsync(assignmentSql, request.Responsibilities.Select(assignment => new
            {
                ProductId = id,
                assignment.TeamMemberId,
                ResponsibilityRole = assignment.ResponsibilityRole.Trim(),
                assignment.Description
            }), transaction);
        }
        transaction.Commit();
        return id;
    }

    public async Task<bool> UpdateAsync(int productId, ProductUpdateRequest request)
    {
        const string sql = """
            UPDATE Products
            SET ProductName = @ProductName,
                Description = @Description,
                BusinessPurpose = @BusinessPurpose,
                LifecycleStatus = @LifecycleStatus,
                CurrentVersion = @CurrentVersion,
                SupportedMarkets = @SupportedMarkets,
                Criticality = @Criticality,
                Technologies = @Technologies,
                Notes = @Notes,
                UpdatedAt = SYSUTCDATETIME()
            WHERE ProductId = @ProductId;
            """;

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var updated = await connection.ExecuteAsync(sql, new
        {
            ProductId = productId,
            request.ProductName,
            request.Description,
            request.BusinessPurpose,
            request.LifecycleStatus,
            request.CurrentVersion,
            request.SupportedMarkets,
            request.Criticality,
            request.Technologies,
            request.Notes
        }, transaction) > 0;
        if (!updated) return false;

        if (request.Responsibilities is not null)
        {
            await connection.ExecuteAsync(
                "DELETE FROM ProductResponsibilities WHERE ProductId = @Id;",
                new { Id = productId }, transaction);
            const string assignmentSql = """
                INSERT INTO ProductResponsibilities (ProductId, TeamMemberId, ResponsibilityRole, Description)
                VALUES (@ProductId, @TeamMemberId, @ResponsibilityRole, @Description);
                """;
            if (request.Responsibilities.Count > 0)
            {
                await connection.ExecuteAsync(assignmentSql, request.Responsibilities.Select(assignment => new
                {
                    ProductId = productId,
                    assignment.TeamMemberId,
                    ResponsibilityRole = assignment.ResponsibilityRole.Trim(),
                    assignment.Description
                }), transaction);
            }
        }
        transaction.Commit();
        return true;
    }

    public async Task<bool> DeleteAsync(int productId)
    {
        const string sql = "DELETE FROM Products WHERE ProductId = @ProductId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { ProductId = productId }) > 0;
    }
}
