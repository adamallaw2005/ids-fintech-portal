using MyWebsite_API.Models;

namespace MyWebsite_API.Services;

public interface IOverviewService
{
    Task<DashboardResponse> GetDashboardAsync();
    Task<ProductDetailsResponse?> GetProductDetailsAsync(int productId);
    Task<ClientDetailsResponse?> GetClientDetailsAsync(int clientId);
}
