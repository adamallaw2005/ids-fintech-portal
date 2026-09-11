using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(string? search, string? status, string? technology);
    Task<Product?> GetByIdAsync(int productId);
    Task<int> CreateAsync(ProductCreateRequest request);
    Task<bool> UpdateAsync(int productId, ProductUpdateRequest request);
    Task<bool> DeleteAsync(int productId);
}
