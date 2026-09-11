using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class RepositoryLinkRepository(IDbConnectionFactory connectionFactory) : IRepositoryLinkRepository
{
    public async Task<IReadOnlyList<RepositoryDetails>> GetAllAsync(int? productId)
    {
        const string sql = """
            SELECT
                r.RepositoryId,
                r.ProductId,
                p.ProductName,
                r.RepositoryName,
                r.GitHubUrl,
                r.MainBranch,
                r.Description,
                r.CreatedAt,
                r.UpdatedAt
            FROM Repositories r
            INNER JOIN Products p ON p.ProductId = r.ProductId
            WHERE (@ProductId IS NULL OR r.ProductId = @ProductId)
            ORDER BY p.ProductName, r.RepositoryName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var repositories = await connection.QueryAsync<RepositoryDetails>(sql, new { ProductId = productId });
        return repositories.AsList();
    }

    public async Task<RepositoryDetails?> GetByIdAsync(int repositoryId)
    {
        const string sql = """
            SELECT
                r.RepositoryId,
                r.ProductId,
                p.ProductName,
                r.RepositoryName,
                r.GitHubUrl,
                r.MainBranch,
                r.Description,
                r.CreatedAt,
                r.UpdatedAt
            FROM Repositories r
            INNER JOIN Products p ON p.ProductId = r.ProductId
            WHERE r.RepositoryId = @RepositoryId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<RepositoryDetails>(sql, new { RepositoryId = repositoryId });
    }

    public async Task<int> CreateAsync(RepositoryRequest request)
    {
        const string sql = """
            INSERT INTO Repositories
            (
                ProductId,
                RepositoryName,
                GitHubUrl,
                MainBranch,
                Description
            )
            OUTPUT INSERTED.RepositoryId
            VALUES
            (
                @ProductId,
                @RepositoryName,
                @GitHubUrl,
                @MainBranch,
                @Description
            );
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, request);
    }

    public async Task<bool> UpdateAsync(int repositoryId, RepositoryRequest request)
    {
        const string sql = """
            UPDATE Repositories
            SET ProductId = @ProductId,
                RepositoryName = @RepositoryName,
                GitHubUrl = @GitHubUrl,
                MainBranch = @MainBranch,
                Description = @Description,
                UpdatedAt = SYSUTCDATETIME()
            WHERE RepositoryId = @RepositoryId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            RepositoryId = repositoryId,
            request.ProductId,
            request.RepositoryName,
            request.GitHubUrl,
            request.MainBranch,
            request.Description
        }) > 0;
    }

    public async Task<bool> DeleteAsync(int repositoryId)
    {
        const string sql = "DELETE FROM Repositories WHERE RepositoryId = @RepositoryId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { RepositoryId = repositoryId }) > 0;
    }
}
