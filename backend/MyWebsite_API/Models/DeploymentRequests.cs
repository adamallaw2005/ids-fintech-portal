using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public sealed class DeploymentRequest
{
    [Range(1, int.MaxValue)]
    public int ClientId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Required, StringLength(50)]
    public string ProductVersion { get; set; } = string.Empty;

    public DateTime? GoLiveDate { get; set; }

    [Required, StringLength(30)]
    public string DeploymentStatus { get; set; } = "Testing";

    [StringLength(30)]
    public string? SupportTier { get; set; }

    public string? ClientSpecificNotes { get; set; }
}

public sealed class DeploymentSummary
{
    public int DeploymentId { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductVersion { get; set; } = string.Empty;
    public DateTime? GoLiveDate { get; set; }
    public string DeploymentStatus { get; set; } = string.Empty;
    public string? SupportTier { get; set; }
    public string? ClientSpecificNotes { get; set; }
    public int EnvironmentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class DeploymentDetails
{
    public int DeploymentId { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductVersion { get; set; } = string.Empty;
    public DateTime? GoLiveDate { get; set; }
    public string DeploymentStatus { get; set; } = string.Empty;
    public string? SupportTier { get; set; }
    public string? ClientSpecificNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
