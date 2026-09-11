using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface IProductResponsibilityRepository
{
    Task<IReadOnlyList<ProductResponsibilityDetails>> GetAllAsync(int? productId, int? teamMemberId);
    Task<ProductResponsibilityDetails?> GetByIdAsync(int responsibilityId);
    Task<int> CreateAsync(ProductResponsibilityRequest request);
    Task<bool> UpdateAsync(int responsibilityId, ProductResponsibilityRequest request);
    Task<bool> DeleteAsync(int responsibilityId);
}
