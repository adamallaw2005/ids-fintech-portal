using System.ComponentModel.DataAnnotations;

namespace MyWebsite_API.Models;

public class ClientRequest
{
    [Required, StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Country { get; set; }

    public string? ContactInformation { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Prospect";

    public string? Notes { get; set; }
}

public sealed class ClientCreateRequest : ClientRequest
{
    [Required]
    public List<ResponsibilityAssignmentRequest> Responsibilities { get; set; } = [];
}

public sealed class ClientUpdateRequest : ClientRequest
{
    public List<ResponsibilityAssignmentRequest>? Responsibilities { get; set; }
}
