using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class DeploymentModuleService(IDeploymentModuleRepository deploymentModuleRepository) : IDeploymentModuleService
{
    public Task<IReadOnlyList<DeploymentModuleDetails>> GetAllAsync(int? deploymentId, int? productId) =>
        deploymentModuleRepository.GetAllAsync(deploymentId, productId);

    public Task<DeploymentModuleDetails?> GetByIdAsync(int deploymentModuleId) =>
        deploymentModuleRepository.GetByIdAsync(deploymentModuleId);

    public Task<int?> CreateAsync(DeploymentModuleRequest request) =>
        deploymentModuleRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int deploymentModuleId, DeploymentModuleRequest request) =>
        deploymentModuleRepository.UpdateAsync(deploymentModuleId, request);

    public Task<bool> DeleteAsync(int deploymentModuleId) =>
        deploymentModuleRepository.DeleteAsync(deploymentModuleId);
}
