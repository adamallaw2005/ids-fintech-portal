using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public interface IOverviewRepository
{
    Task<DashboardResponse> GetDashboardAsync();
    Task<ProductDetailsResponse?> GetProductDetailsAsync(int productId);
    Task<ClientDetailsResponse?> GetClientDetailsAsync(int clientId);
}
