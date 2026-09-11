using MyWebsite_API.Models;

namespace MyWebsite_API.Services;

public interface IProductModuleService
{
    Task<IReadOnlyList<ProductModuleDetails>> GetAllAsync(int? productId, string? status);
    Task<ProductModuleDetails?> GetByIdAsync(int moduleId);
    Task<int> CreateAsync(ProductModuleRequest request);
    Task<bool> UpdateAsync(int moduleId, ProductModuleRequest request);
    Task<bool> DeleteAsync(int moduleId);
}
