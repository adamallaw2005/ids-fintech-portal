using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface IDocumentRepository
{
    Task<IReadOnlyList<DocumentDetails>> GetAllAsync(int? productId, string? documentType);
    Task<DocumentDetails?> GetByIdAsync(int documentId);
    Task<int> CreateAsync(DocumentRequest request);
    Task<bool> UpdateAsync(int documentId, DocumentRequest request);
    Task<bool> DeleteAsync(int documentId);
}
