using MyWebsite_API.Models;

namespace MyWebsite_API.Services;

public interface IDeploymentService
{
    Task<IReadOnlyList<DeploymentSummary>> GetAllAsync(
        int? clientId,
        int? productId,
        string? version,
        string? status,
        string? environment);

    Task<DeploymentDetails?> GetByIdAsync(int deploymentId);
    Task<int> CreateAsync(DeploymentRequest request);
    Task<bool> UpdateAsync(int deploymentId, DeploymentRequest request);
    Task<bool> DeleteAsync(int deploymentId);
}
