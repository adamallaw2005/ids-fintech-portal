using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public sealed class TeamMemberRequest
{
    public List<TeamAssignmentRequest>? ProductAssignments { get; set; }
    public List<TeamAssignmentRequest>? ClientAssignments { get; set; }

    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(150)]
    public string? JobTitle { get; set; }

    [StringLength(150)]
    public string? DepartmentTeam { get; set; }

    [EmailAddress, StringLength(255)]
    public string? Email { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Active";
}

public sealed class TeamAssignmentRequest
{
    [Range(1, int.MaxValue)]
    public int TargetId { get; set; }

    [Required, StringLength(100)]
    public string ResponsibilityRole { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class TeamMemberSummary
{
    public int TeamMemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? DepartmentTeam { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ProductResponsibilityCount { get; set; }
    public int ClientResponsibilityCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class ProductResponsibilityRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int TeamMemberId { get; set; }

    [Required, StringLength(100)]
    public string ResponsibilityRole { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class ProductResponsibilityDetails
{
    public int ResponsibilityId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TeamMemberId { get; set; }
    public string TeamMemberName { get; set; } = string.Empty;
    public string ResponsibilityRole { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ClientResponsibilityRequest
{
    [Range(1, int.MaxValue)]
    public int ClientId { get; set; }

    [Range(1, int.MaxValue)]
    public int TeamMemberId { get; set; }

    [Required, StringLength(100)]
    public string ResponsibilityRole { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class ClientResponsibilityDetails
{
    public int ClientResponsibilityId { get; set; }
    public int ClientId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int TeamMemberId { get; set; }
    public string TeamMemberName { get; set; } = string.Empty;
    public string ResponsibilityRole { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
