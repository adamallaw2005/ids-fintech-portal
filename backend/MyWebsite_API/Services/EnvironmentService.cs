using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class EnvironmentService(IEnvironmentRepository environmentRepository) : IEnvironmentService
{
    public Task<IReadOnlyList<EnvironmentDetails>> GetAllAsync(int? deploymentId, string? type) =>
        environmentRepository.GetAllAsync(deploymentId, type);

    public Task<EnvironmentDetails?> GetByIdAsync(int environmentId) =>
        environmentRepository.GetByIdAsync(environmentId);

    public Task<int> CreateAsync(EnvironmentRequest request) =>
        environmentRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int environmentId, EnvironmentRequest request) =>
        environmentRepository.UpdateAsync(environmentId, request);

    public Task<bool> DeleteAsync(int environmentId) =>
        environmentRepository.DeleteAsync(environmentId);
}
