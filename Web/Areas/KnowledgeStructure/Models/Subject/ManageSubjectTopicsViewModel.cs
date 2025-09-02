using LinaSys.KnowledgeStructure.Application.Subject.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Subject;

public class ManageSubjectTopicsViewModel
{
    public long SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public string? SubjectDescription { get; set; }

    public List<TopicReferenceDto> AssignedTopics { get; set; } = [];

    public List<SelectListItem> AvailableTopics { get; set; } = [];

    public long? SelectedStructureTopicId { get; set; }
}
