using LinaSys.Diagnostics.Application.Form.Queries;
using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;

namespace LinaSys.Web.Areas.Diagnostics.Models.Forms;

public class FormBuilderViewModel
{
    public long FormId { get; set; }

    public string FormName { get; set; } = string.Empty;

    public List<QuestionsDto> Questions { get; set; } = [];

    public Dictionary<long, string> AvailableBlocks { get; set; } = [];

    public List<KnowledgeStructureHierarchyDto> KnowledgeStructures { get; set; } = [];
}

public class AddQuestionToFormViewModel
{
    public long FormId { get; set; }

    public long? TopicId { get; set; }

    public long BlockId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public int AnswerType { get; set; }

    public int QuestionPhase { get; set; }

    public bool IsUsedForMentoringPlan { get; set; }

    public bool IsUsedForDiagnosis { get; set; }

    public int Order { get; set; }
}

public class RemoveQuestionFromFormViewModel
{
    public long FormId { get; set; }

    public long QuestionId { get; set; }
}

public class ReorderQuestionsViewModel
{
    public long FormId { get; set; }

    public List<QuestionOrderItem> Questions { get; set; } = [];
}

public class QuestionOrderItem
{
    public long QuestionId { get; set; }

    public int Order { get; set; }
}
