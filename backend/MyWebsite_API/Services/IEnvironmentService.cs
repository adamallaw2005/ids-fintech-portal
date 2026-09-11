using MyWebsite_API.Models;

namespace MyWebsite_API.Services;

public interface IEnvironmentService
{
    Task<IReadOnlyList<EnvironmentDetails>> GetAllAsync(int? deploymentId, string? type);
    Task<EnvironmentDetails?> GetByIdAsync(int environmentId);
    Task<int> CreateAsync(EnvironmentRequest request);
    Task<bool> UpdateAsync(int environmentId, EnvironmentRequest request);
    Task<bool> DeleteAsync(int environmentId);
}
