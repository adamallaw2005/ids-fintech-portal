using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public sealed class EnvironmentRequest
{
    [Range(1, int.MaxValue)]
    public int DeploymentId { get; set; }

    [Required, StringLength(100)]
    public string EnvironmentName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string EnvironmentType { get; set; } = "Testing";

    public string? Purpose { get; set; }
    public string? ServerName { get; set; }
    public string? OperatingSystem { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? DatabaseInformation { get; set; }
    public string? MonitoringLink { get; set; }
    public string? AccessInstructionsReference { get; set; }
    public string? Notes { get; set; }
}

public sealed class EnvironmentDetails
{
    public int EnvironmentId { get; set; }
    public int DeploymentId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductVersion { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
    public string EnvironmentType { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public string? ServerName { get; set; }
    public string? OperatingSystem { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? DatabaseInformation { get; set; }
    public string? MonitoringLink { get; set; }
    public string? AccessInstructionsReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
