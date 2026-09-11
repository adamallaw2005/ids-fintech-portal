using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public class ProductRequest
{
    [Required, StringLength(150)]
    public string ProductName { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? BusinessPurpose { get; set; }

    [Required, StringLength(30)]
    public string LifecycleStatus { get; set; } = "Planned";

    [StringLength(50)]
    public string? CurrentVersion { get; set; }
    [StringLength(300)]
    public string? SupportedMarkets { get; set; }

    [Required, StringLength(20)]
    public string Criticality { get; set; } = "Medium";

    [StringLength(500)]
    public string? Technologies { get; set; }
    public string? Notes { get; set; }
}

public sealed class ProductCreateRequest : ProductRequest
{
    [Required]
    public List<ResponsibilityAssignmentRequest> Responsibilities { get; set; } = [];
}

public sealed class ProductUpdateRequest : ProductRequest
{
    public List<ResponsibilityAssignmentRequest>? Responsibilities { get; set; }
}
