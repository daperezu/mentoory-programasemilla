using LinaSys.KnowledgeStructure.Application.Module.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Module;

public class ManageModuleKnowledgeStructuresViewModel
{
    public long ModuleId { get; set; }

    public string ModuleName { get; set; } = string.Empty;

    public List<KnowledgeStructureAssignmentDto> AssignedKnowledgeStructures { get; set; } = [];

    public List<SelectListItem> AvailableKnowledgeStructures { get; set; } = [];

    public long? SelectedKnowledgeStructureId { get; set; }
}
