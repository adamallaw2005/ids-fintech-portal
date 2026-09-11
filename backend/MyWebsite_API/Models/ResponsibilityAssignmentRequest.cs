using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public sealed class ResponsibilityAssignmentRequest
{
    [Range(1, int.MaxValue)]
    public int TeamMemberId { get; set; }

    [Required, StringLength(100)]
    public string ResponsibilityRole { get; set; } = string.Empty;

    public string? Description { get; set; }
}
