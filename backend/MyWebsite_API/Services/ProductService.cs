using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class ProductService(IProductRepository productRepository) : IProductService
{
    public Task<IReadOnlyList<Product>> GetAllAsync(string? search, string? status, string? technology) =>
        productRepository.GetAllAsync(search, status, technology);

    public Task<Product?> GetByIdAsync(int productId) => productRepository.GetByIdAsync(productId);

    public Task<int> CreateAsync(ProductCreateRequest request) => productRepository.CreateAsync(request);

    public Task<bool> UpdateAsync(int productId, ProductUpdateRequest request) => productRepository.UpdateAsync(productId, request);

    public Task<bool> DeleteAsync(int productId) => productRepository.DeleteAsync(productId);
}
