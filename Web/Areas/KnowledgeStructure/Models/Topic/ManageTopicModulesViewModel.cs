using LinaSys.KnowledgeStructure.Application.Topic.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Topic;

public class ManageTopicModulesViewModel
{
    public long TopicId { get; set; }

    public string TopicName { get; set; } = string.Empty;

    public List<ModuleAssignmentDto> AssignedModules { get; set; } = [];

    public List<SelectListItem> AvailableModules { get; set; } = [];

    public long? SelectedStructureModuleId { get; set; }
}
