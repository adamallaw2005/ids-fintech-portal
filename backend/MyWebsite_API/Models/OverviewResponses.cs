namespace MyWebsite_API.Models;

public sealed class DashboardResponse
{
    public int ProductsCount { get; set; }
    public int ActiveProductsCount { get; set; }
    public int ClientsCount { get; set; }
    public int DeploymentsCount { get; set; }
    public int TeamMembersCount { get; set; }
    public IReadOnlyList<RecentProduct> RecentlyUpdatedProducts { get; set; } = [];
}

public sealed class RecentProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string LifecycleStatus { get; set; } = string.Empty;
    public string? CurrentVersion { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class ProductDetailsResponse
{
    public Product Product { get; set; } = new();
    public IReadOnlyList<ProductModuleDetails> Modules { get; set; } = [];
    public IReadOnlyList<DeploymentSummary> Deployments { get; set; } = [];
    public IReadOnlyList<ProductResponsibilityDetails> Responsibilities { get; set; } = [];
    public IReadOnlyList<RepositoryDetails> Repositories { get; set; } = [];
    public IReadOnlyList<DocumentDetails> Documents { get; set; } = [];
}

public sealed class ClientDetailsResponse
{
    public Client Client { get; set; } = new();
    public IReadOnlyList<DeploymentSummary> Deployments { get; set; } = [];
    public IReadOnlyList<EnvironmentDetails> Environments { get; set; } = [];
    public IReadOnlyList<ClientResponsibilityDetails> Responsibilities { get; set; } = [];
}
