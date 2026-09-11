using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class ProductModuleService(IProductModuleRepository productModuleRepository) : IProductModuleService
{
    public Task<IReadOnlyList<ProductModuleDetails>> GetAllAsync(int? productId, string? status) =>
        productModuleRepository.GetAllAsync(productId, status);

    public Task<ProductModuleDetails?> GetByIdAsync(int moduleId) =>
        productModuleRepository.GetByIdAsync(moduleId);

    public Task<int> CreateAsync(ProductModuleRequest request) =>
        productModuleRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int moduleId, ProductModuleRequest request) =>
        productModuleRepository.UpdateAsync(moduleId, request);

    public Task<bool> DeleteAsync(int moduleId) =>
        productModuleRepository.DeleteAsync(moduleId);
}
