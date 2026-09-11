using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public sealed class ProductModuleRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Required, StringLength(200)]
    public string ModuleName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "Active";
}

public sealed class ProductModuleDetails
{
    public int ModuleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
