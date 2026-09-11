using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface IRepositoryLinkRepository
{
    Task<IReadOnlyList<RepositoryDetails>> GetAllAsync(int? productId);
    Task<RepositoryDetails?> GetByIdAsync(int repositoryId);
    Task<int> CreateAsync(RepositoryRequest request);
    Task<bool> UpdateAsync(int repositoryId, RepositoryRequest request);
    Task<bool> DeleteAsync(int repositoryId);
}
