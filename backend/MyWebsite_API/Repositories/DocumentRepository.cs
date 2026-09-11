using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class DocumentRepository(IDbConnectionFactory connectionFactory) : IDocumentRepository
{
    public async Task<IReadOnlyList<DocumentDetails>> GetAllAsync(int? productId, string? documentType)
    {
        const string sql = """
            SELECT
                d.DocumentId,
                d.ProductId,
                p.ProductName,
                d.DocumentName,
                d.DocumentType,
                d.Description,
                d.UrlFileReference,
                d.LastUpdatedDate,
                d.CreatedAt,
                d.UpdatedAt
            FROM Documents d
            INNER JOIN Products p ON p.ProductId = d.ProductId
            WHERE (@ProductId IS NULL OR d.ProductId = @ProductId)
              AND (@DocumentType IS NULL OR d.DocumentType = @DocumentType)
            ORDER BY p.ProductName, d.DocumentType, d.DocumentName;
            """;

        using var connection = connectionFactory.CreateConnection();
        var documents = await connection.QueryAsync<DocumentDetails>(sql, new
        {
            ProductId = productId,
            DocumentType = documentType
        });

        return documents.AsList();
    }

    public async Task<DocumentDetails?> GetByIdAsync(int documentId)
    {
        const string sql = """
            SELECT
                d.DocumentId,
                d.ProductId,
                p.ProductName,
                d.DocumentName,
                d.DocumentType,
                d.Description,
                d.UrlFileReference,
                d.LastUpdatedDate,
                d.CreatedAt,
                d.UpdatedAt
            FROM Documents d
            INNER JOIN Products p ON p.ProductId = d.ProductId
            WHERE d.DocumentId = @DocumentId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DocumentDetails>(sql, new { DocumentId = documentId });
    }

    public async Task<int> CreateAsync(DocumentRequest request)
    {
        const string sql = """
            INSERT INTO Documents
            (
                ProductId,
                DocumentName,
                DocumentType,
                Description,
                UrlFileReference,
                LastUpdatedDate
            )
            OUTPUT INSERTED.DocumentId
            VALUES
            (
                @ProductId,
                @DocumentName,
                @DocumentType,
                @Description,
                @UrlFileReference,
                @LastUpdatedDate
            );
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, request);
    }

    public async Task<bool> UpdateAsync(int documentId, DocumentRequest request)
    {
        const string sql = """
            UPDATE Documents
            SET ProductId = @ProductId,
                DocumentName = @DocumentName,
                DocumentType = @DocumentType,
                Description = @Description,
                UrlFileReference = @UrlFileReference,
                LastUpdatedDate = @LastUpdatedDate,
                UpdatedAt = SYSUTCDATETIME()
            WHERE DocumentId = @DocumentId;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            DocumentId = documentId,
            request.ProductId,
            request.DocumentName,
            request.DocumentType,
            request.Description,
            request.UrlFileReference,
            request.LastUpdatedDate
        }) > 0;
    }

    public async Task<bool> DeleteAsync(int documentId)
    {
        const string sql = "DELETE FROM Documents WHERE DocumentId = @DocumentId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { DocumentId = documentId }) > 0;
    }
}
