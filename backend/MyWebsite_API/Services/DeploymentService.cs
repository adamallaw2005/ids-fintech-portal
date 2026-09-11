using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class DeploymentService(IDeploymentRepository deploymentRepository) : IDeploymentService
{
    public Task<IReadOnlyList<DeploymentSummary>> GetAllAsync(
        int? clientId,
        int? productId,
        string? version,
        string? status,
        string? environment) =>
        deploymentRepository.GetAllAsync(clientId, productId, version, status, environment);

    public Task<DeploymentDetails?> GetByIdAsync(int deploymentId) =>
        deploymentRepository.GetByIdAsync(deploymentId);

    public Task<int> CreateAsync(DeploymentRequest request) =>
        deploymentRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int deploymentId, DeploymentRequest request) =>
        deploymentRepository.UpdateAsync(deploymentId, request);

    public Task<bool> DeleteAsync(int deploymentId) =>
        deploymentRepository.DeleteAsync(deploymentId);
}
