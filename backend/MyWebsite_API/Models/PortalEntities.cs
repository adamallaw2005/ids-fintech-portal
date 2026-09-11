namespace MyWebsite_API.Models;

public sealed class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BusinessPurpose { get; set; }
    public string LifecycleStatus { get; set; } = string.Empty;
    public string? CurrentVersion { get; set; }
    public string? SupportedMarkets { get; set; }
    public string Criticality { get; set; } = string.Empty;
    public string? Technologies { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class Client
{
    public int ClientId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? ContactInformation { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class TeamMember
{
    public int TeamMemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? DepartmentTeam { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

