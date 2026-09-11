using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class DocumentService(IDocumentRepository documentRepository) : IDocumentService
{
    public Task<IReadOnlyList<DocumentDetails>> GetAllAsync(int? productId, string? documentType) =>
        documentRepository.GetAllAsync(productId, documentType);

    public Task<DocumentDetails?> GetByIdAsync(int documentId) =>
        documentRepository.GetByIdAsync(documentId);

    public Task<int> CreateAsync(DocumentRequest request) =>
        documentRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int documentId, DocumentRequest request) =>
        documentRepository.UpdateAsync(documentId, request);

    public Task<bool> DeleteAsync(int documentId) =>
        documentRepository.DeleteAsync(documentId);
}
