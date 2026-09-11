using MyWebsite_API.Models;

namespace MyWebsite_API.Services;

public interface IDeploymentModuleService
{
    Task<IReadOnlyList<DeploymentModuleDetails>> GetAllAsync(int? deploymentId, int? productId);
    Task<DeploymentModuleDetails?> GetByIdAsync(int deploymentModuleId);
    Task<int?> CreateAsync(DeploymentModuleRequest request);
    Task<bool> UpdateAsync(int deploymentModuleId, DeploymentModuleRequest request);
    Task<bool> DeleteAsync(int deploymentModuleId);
}
