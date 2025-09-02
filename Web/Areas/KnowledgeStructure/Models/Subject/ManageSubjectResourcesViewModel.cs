using LinaSys.KnowledgeStructure.Application.Subject.DTOs;

namespace LinaSys.Web.Areas.KnowledgeStructure.Models.Subject;

public class ManageSubjectResourcesViewModel
{
    public long SubjectId { get; set; }

    public string SubjectTitle { get; set; } = string.Empty;

    public List<SubjectResourceDto> Resources { get; set; } = [];
}
