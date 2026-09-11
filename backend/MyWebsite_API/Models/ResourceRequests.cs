using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public sealed class RepositoryRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Required, StringLength(150)]
    public string RepositoryName { get; set; } = string.Empty;

    [Required, Url, StringLength(500)]
    public string GitHubUrl { get; set; } = string.Empty;

    [StringLength(100)]
    public string? MainBranch { get; set; }

    public string? Description { get; set; }
}

public sealed class RepositoryDetails
{
    public int RepositoryId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public string? GitHubUrl { get; set; }
    public string? MainBranch { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class DocumentRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Required, StringLength(200)]
    public string DocumentName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string DocumentType { get; set; } = "Technical Documentation";

    public string? Description { get; set; }
    [Required, StringLength(500)]
    public string UrlFileReference { get; set; } = string.Empty;
    public DateTime? LastUpdatedDate { get; set; }
}

public sealed class DocumentDetails
{
    public int DocumentId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? UrlFileReference { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
