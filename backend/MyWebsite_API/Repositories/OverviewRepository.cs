using Dapper;
using MyWebsite_API.Data;
using MyWebsite_API.Models;

namespace MyWebsite_API.Repositories;

public sealed class OverviewRepository(IDbConnectionFactory connectionFactory) : IOverviewRepository
{
    public async Task<DashboardResponse> GetDashboardAsync()
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM Products) AS ProductsCount,
                (SELECT COUNT(*) FROM Products WHERE LifecycleStatus = 'Active') AS ActiveProductsCount,
                (SELECT COUNT(*) FROM Clients) AS ClientsCount,
                (SELECT COUNT(*) FROM Deployments) AS DeploymentsCount,
                (SELECT COUNT(*) FROM TeamMembers) AS TeamMembersCount;

            SELECT TOP 5
                ProductId,
                ProductName,
                LifecycleStatus,
                CurrentVersion,
                COALESCE(UpdatedAt, CreatedAt) AS UpdatedAt
            FROM Products
            ORDER BY COALESCE(UpdatedAt, CreatedAt) DESC, ProductName;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var results = await connection.QueryMultipleAsync(sql);

        var dashboard = await results.ReadSingleAsync<DashboardResponse>();
        dashboard.RecentlyUpdatedProducts = (await results.ReadAsync<RecentProduct>()).AsList();
        return dashboard;
    }

    public async Task<ProductDetailsResponse?> GetProductDetailsAsync(int productId)
    {
        const string sql = """
            SELECT ProductId, ProductName, Description, BusinessPurpose, LifecycleStatus,
                   CurrentVersion, SupportedMarkets, Criticality, Technologies, Notes,
                   CreatedAt, UpdatedAt
            FROM Products
            WHERE ProductId = @ProductId;

            SELECT
                pm.ModuleId,
                pm.ProductId,
                p.ProductName,
                pm.ModuleName,
                pm.Description,
                pm.Status,
                pm.CreatedAt,
                pm.UpdatedAt
            FROM ProductModules pm
            INNER JOIN Products p ON p.ProductId = pm.ProductId
            WHERE pm.ProductId = @ProductId
            ORDER BY pm.ModuleName;

            SELECT
                d.DeploymentId,
                d.ClientId,
                c.CompanyName AS ClientName,
                d.ProductId,
                p.ProductName,
                d.ProductVersion,
                d.GoLiveDate,
                d.DeploymentStatus,
                d.SupportTier,
                d.ClientSpecificNotes,
                COUNT(e.EnvironmentId) AS EnvironmentCount,
                d.CreatedAt,
                d.UpdatedAt
            FROM Deployments d
            INNER JOIN Clients c ON c.ClientId = d.ClientId
            INNER JOIN Products p ON p.ProductId = d.ProductId
            LEFT JOIN Environments e ON e.DeploymentId = d.DeploymentId
            WHERE d.ProductId = @ProductId
            GROUP BY
                d.DeploymentId, d.ClientId, c.CompanyName, d.ProductId, p.ProductName,
                d.ProductVersion, d.GoLiveDate, d.DeploymentStatus, d.SupportTier,
                d.ClientSpecificNotes, d.CreatedAt, d.UpdatedAt
            ORDER BY c.CompanyName, d.ProductVersion;

            SELECT
                pr.ResponsibilityId,
                pr.ProductId,
                p.ProductName,
                pr.TeamMemberId,
                tm.FullName AS TeamMemberName,
                pr.ResponsibilityRole,
                pr.Description,
                pr.CreatedAt
            FROM ProductResponsibilities pr
            INNER JOIN Products p ON p.ProductId = pr.ProductId
            INNER JOIN TeamMembers tm ON tm.TeamMemberId = pr.TeamMemberId
            WHERE pr.ProductId = @ProductId
            ORDER BY tm.FullName, pr.ResponsibilityRole;

            SELECT
                r.RepositoryId,
                r.ProductId,
                p.ProductName,
                r.RepositoryName,
                r.GitHubUrl,
                r.MainBranch,
                r.Description,
                r.CreatedAt,
                r.UpdatedAt
            FROM Repositories r
            INNER JOIN Products p ON p.ProductId = r.ProductId
            WHERE r.ProductId = @ProductId
            ORDER BY r.RepositoryName;

            SELECT
                d.DocumentId,
                d.ProductId,
                p.ProductName,
                d.DocumentName,
                d.DocumentType,
                d.Description,
                d.UrlFileReference,
                d.LastUpdatedDate,
                d.CreatedAt,
                d.UpdatedAt
            FROM Documents d
            INNER JOIN Products p ON p.ProductId = d.ProductId
            WHERE d.ProductId = @ProductId
            ORDER BY d.DocumentType, d.DocumentName;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var results = await connection.QueryMultipleAsync(sql, new { ProductId = productId });

        var product = await results.ReadSingleOrDefaultAsync<Product>();
        if (product is null)
        {
            return null;
        }

        return new ProductDetailsResponse
        {
            Product = product,
            Modules = (await results.ReadAsync<ProductModuleDetails>()).AsList(),
            Deployments = (await results.ReadAsync<DeploymentSummary>()).AsList(),
            Responsibilities = (await results.ReadAsync<ProductResponsibilityDetails>()).AsList(),
            Repositories = (await results.ReadAsync<RepositoryDetails>()).AsList(),
            Documents = (await results.ReadAsync<DocumentDetails>()).AsList()
        };
    }

    public async Task<ClientDetailsResponse?> GetClientDetailsAsync(int clientId)
    {
        const string sql = """
            SELECT ClientId, CompanyName, Country, ContactInformation,
                   Status, Notes, CreatedAt, UpdatedAt
            FROM Clients
            WHERE ClientId = @ClientId;

            SELECT
                d.DeploymentId,
                d.ClientId,
                c.CompanyName AS ClientName,
                d.ProductId,
                p.ProductName,
                d.ProductVersion,
                d.GoLiveDate,
                d.DeploymentStatus,
                d.SupportTier,
                d.ClientSpecificNotes,
                COUNT(e.EnvironmentId) AS EnvironmentCount,
                d.CreatedAt,
                d.UpdatedAt
            FROM Deployments d
            INNER JOIN Clients c ON c.ClientId = d.ClientId
            INNER JOIN Products p ON p.ProductId = d.ProductId
            LEFT JOIN Environments e ON e.DeploymentId = d.DeploymentId
            WHERE d.ClientId = @ClientId
            GROUP BY
                d.DeploymentId, d.ClientId, c.CompanyName, d.ProductId, p.ProductName,
                d.ProductVersion, d.GoLiveDate, d.DeploymentStatus, d.SupportTier,
                d.ClientSpecificNotes, d.CreatedAt, d.UpdatedAt
            ORDER BY p.ProductName, d.ProductVersion;

            SELECT
                e.EnvironmentId,
                e.DeploymentId,
                c.CompanyName AS ClientName,
                p.ProductName,
                d.ProductVersion,
                e.EnvironmentName,
                e.EnvironmentType,
                e.Purpose,
                e.ServerName,
                e.OperatingSystem,
                e.ApplicationUrl,
                e.DatabaseInformation,
                e.MonitoringLink,
                e.AccessInstructionsReference,
                e.Notes,
                e.CreatedAt,
                e.UpdatedAt
            FROM Environments e
            INNER JOIN Deployments d ON d.DeploymentId = e.DeploymentId
            INNER JOIN Clients c ON c.ClientId = d.ClientId
            INNER JOIN Products p ON p.ProductId = d.ProductId
            WHERE d.ClientId = @ClientId
            ORDER BY p.ProductName, d.ProductVersion, e.EnvironmentType;

            SELECT
                cr.ClientResponsibilityId,
                cr.ClientId,
                c.CompanyName,
                cr.TeamMemberId,
                tm.FullName AS TeamMemberName,
                cr.ResponsibilityRole,
                cr.Description,
                cr.CreatedAt
            FROM ClientResponsibilities cr
            INNER JOIN Clients c ON c.ClientId = cr.ClientId
            INNER JOIN TeamMembers tm ON tm.TeamMemberId = cr.TeamMemberId
            WHERE cr.ClientId = @ClientId
            ORDER BY tm.FullName, cr.ResponsibilityRole;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var results = await connection.QueryMultipleAsync(sql, new { ClientId = clientId });

        var client = await results.ReadSingleOrDefaultAsync<Client>();
        if (client is null)
        {
            return null;
        }

        return new ClientDetailsResponse
        {
            Client = client,
            Deployments = (await results.ReadAsync<DeploymentSummary>()).AsList(),
            Environments = (await results.ReadAsync<EnvironmentDetails>()).AsList(),
            Responsibilities = (await results.ReadAsync<ClientResponsibilityDetails>()).AsList()
        };
    }
}
