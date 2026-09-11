using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class ProductResponsibilityRepository(IDbConnectionFactory connectionFactory) : IProductResponsibilityRepository
{
    public async Task<IReadOnlyList<ProductResponsibilityDetails>> GetAllAsync(int? productId, int? teamMemberId)
    {
        const string sql = """
            SELECT
                pr.ResponsibilityId,
                pr.ProductId,
                p.ProductName,
                pr.TeamMemberId,
                tm.FullName AS TeamMemberName,
                pr.ResponsibilityRole,
                pr.Description,
                pr.CreatedAt
            FROM ProductResponsibilities pr
            INNER JOIN Products p ON p.ProductId = pr.ProductId
            INNER JOIN TeamMembers tm ON tm.TeamMemberId = pr.TeamMemberId
            WHERE (@ProductId IS NULL OR pr.ProductId = @ProductId)
              AND (@TeamMemberId IS NULL OR pr.TeamMemberId = @TeamMemberId)
            ORDER BY p.ProductName, tm.FullName, pr.ResponsibilityRole;
            """;

        using var connection = connectionFactory.CreateConnection();
        var responsibilities = await connection.QueryAsync<ProductResponsibilityDetails>(sql, new
        {
            ProductId = productId,
            TeamMemberId = teamMemberId
        });

        return responsibilities.AsList();
    }

    public async Task<ProductResponsibilityDetails?> GetByIdAsync(int responsibilityId)
    {
        const string sql = """
            SELECT
                pr.ResponsibilityId,
                pr.ProductId,
                p.ProductName,
                pr.TeamMemberId,
                tm.FullName AS TeamMemberName,
                pr.ResponsibilityRole,
                pr.Description,
                pr.CreatedAt
            FROM ProductResponsibilities pr
            INNER JOIN Products p ON p.ProductId = pr.ProductId
            INNER JOIN TeamMembers tm ON tm.TeamMemberId = pr.TeamMemberId
            WHERE pr.ResponsibilityId = @ResponsibilityId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProductResponsibilityDetails>(sql, new { ResponsibilityId = responsibilityId });
    }

    public async Task<int> CreateAsync(ProductResponsibilityRequest request)
    {
        const string sql = """
            INSERT INTO ProductResponsibilities
            (
                ProductId,
                TeamMemberId,
                ResponsibilityRole,
                Description
            )
            OUTPUT INSERTED.ResponsibilityId
            VALUES
            (
                @ProductId,
                @TeamMemberId,
                @ResponsibilityRole,
                @Description
            );
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, request);
    }

    public async Task<bool> UpdateAsync(int responsibilityId, ProductResponsibilityRequest request)
    {
        const string sql = """
            UPDATE ProductResponsibilities
            SET ProductId = @ProductId,
                TeamMemberId = @TeamMemberId,
                ResponsibilityRole = @ResponsibilityRole,
                Description = @Description
            WHERE ResponsibilityId = @ResponsibilityId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            ResponsibilityId = responsibilityId,
            request.ProductId,
            request.TeamMemberId,
            request.ResponsibilityRole,
            request.Description
        }) > 0;
    }

    public async Task<bool> DeleteAsync(int responsibilityId)
    {
        const string sql = "DELETE FROM ProductResponsibilities WHERE ResponsibilityId = @ResponsibilityId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { ResponsibilityId = responsibilityId }) > 0;
    }
}
