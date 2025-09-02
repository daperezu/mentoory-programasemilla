using LinaSys.Auth.Application.Queries.Context;

namespace LinaSys.Web.Models.Context;

public class ContextSelectionViewModel
{
    public long? CurrentIncubatorId { get; set; }
    public long? CurrentProjectId { get; set; }
    public string? CurrentRole { get; set; }
    public List<UserIncubatorViewModel> Incubators { get; set; } = [];
    public List<UserProjectDto> Projects { get; set; } = [];
    public List<string> Roles { get; set; } = [];
    public long? SelectedIncubatorId { get; set; }
    public long? SelectedProjectId { get; set; }
    public string? SelectedRole { get; set; }
}

public class SelectContextRequest
{
    public long? IncubatorId { get; set; }
    public long? ProjectId { get; set; }
    public string? Role { get; set; }
}

public class UserIncubatorViewModel
{
    public long IncubatorId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ValidateContextRequest
{
    public long? IncubatorId { get; set; }
    public long? ProjectId { get; set; }
    public string? Role { get; set; }
}
