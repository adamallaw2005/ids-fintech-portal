using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class RepositoryLinkService(IRepositoryLinkRepository repository) : IRepositoryLinkService
{
    public Task<IReadOnlyList<RepositoryDetails>> GetAllAsync(int? productId) =>
        repository.GetAllAsync(productId);

    public Task<RepositoryDetails?> GetByIdAsync(int repositoryId) =>
        repository.GetByIdAsync(repositoryId);

    public Task<int> CreateAsync(RepositoryRequest request) =>
        repository.CreateAsync(request);

    public Task<bool> UpdateAsync(int repositoryId, RepositoryRequest request) =>
        repository.UpdateAsync(repositoryId, request);

    public Task<bool> DeleteAsync(int repositoryId) =>
        repository.DeleteAsync(repositoryId);
}
