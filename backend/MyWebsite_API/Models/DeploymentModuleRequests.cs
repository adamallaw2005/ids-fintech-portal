using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public sealed class DeploymentModuleRequest
{
    [Range(1, int.MaxValue)]
    public int DeploymentId { get; set; }

    [Range(1, int.MaxValue)]
    public int ModuleId { get; set; }
}

public sealed class DeploymentModuleDetails
{
    public int DeploymentModuleId { get; set; }
    public int DeploymentId { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductVersion { get; set; } = string.Empty;
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string? ModuleDescription { get; set; }
    public string ModuleStatus { get; set; } = string.Empty;
}
