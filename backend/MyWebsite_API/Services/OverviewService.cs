using MyWebsite_API.Models;
using MyWebsite_API.Repositories;

namespace MyWebsite_API.Services;

public sealed class OverviewService(IOverviewRepository overviewRepository) : IOverviewService
{
    public Task<DashboardResponse> GetDashboardAsync() =>
        overviewRepository.GetDashboardAsync();

    public Task<ProductDetailsResponse?> GetProductDetailsAsync(int productId) =>
        overviewRepository.GetProductDetailsAsync(productId);

    public Task<ClientDetailsResponse?> GetClientDetailsAsync(int clientId) =>
        overviewRepository.GetClientDetailsAsync(clientId);
}
