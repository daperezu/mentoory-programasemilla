using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.BusinessIncubator.Application.Project.DTOs;

public class ProjectInvitationDto
{
    public long Id { get; set; }

    public Guid ExternalId { get; set; }

    public Guid ProjectExternalId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string IdentificationNumber { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public ProjectInvitationStatus Status { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}

public class ProjectInvitationDetailsDto
{
    public long Id { get; set; }

    public Guid ExternalId { get; set; }

    public Guid ProjectExternalId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string IdentificationNumber { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public ProjectInvitationStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsExpired { get; set; }
}
