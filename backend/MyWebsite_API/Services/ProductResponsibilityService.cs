using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class ProductResponsibilityService(IProductResponsibilityRepository responsibilityRepository) : IProductResponsibilityService
{
    public Task<IReadOnlyList<ProductResponsibilityDetails>> GetAllAsync(int? productId, int? teamMemberId) =>
        responsibilityRepository.GetAllAsync(productId, teamMemberId);

    public Task<ProductResponsibilityDetails?> GetByIdAsync(int responsibilityId) =>
        responsibilityRepository.GetByIdAsync(responsibilityId);

    public Task<int> CreateAsync(ProductResponsibilityRequest request) =>
        responsibilityRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int responsibilityId, ProductResponsibilityRequest request) =>
        responsibilityRepository.UpdateAsync(responsibilityId, request);

    public Task<bool> DeleteAsync(int responsibilityId) =>
        responsibilityRepository.DeleteAsync(responsibilityId);
}
