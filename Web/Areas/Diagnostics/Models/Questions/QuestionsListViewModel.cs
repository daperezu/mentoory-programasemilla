using Microsoft.Extensions.Primitives;

namespace LinaSys.Web.Areas.Diagnostics.Models.Questions;

public sealed record QuestionsListViewModel
{
    public Dictionary<int, string> AnswerTypes { get; init; }

    public Dictionary<long, string> Blocks { get; init; }

    public Dictionary<int, string> QuestionPhases { get; init; }

    public Dictionary<long, QuestionListTopicHierarchyViewModel> Topics { get; init; }

    public Dictionary<long, string> KnowledgeStructures { get; init; }

    public Dictionary<long, QuestionListSubjectHierarchyViewModel> Subjects { get; init; }
}

public sealed record QuestionListTopicHierarchyViewModel(
    string KnowledgeStructure,
    string Module,
    string Topic);

public sealed record QuestionListSubjectHierarchyViewModel(
    string KnowledgeStructure,
    string Module,
    string Topic,
    string Subject);
