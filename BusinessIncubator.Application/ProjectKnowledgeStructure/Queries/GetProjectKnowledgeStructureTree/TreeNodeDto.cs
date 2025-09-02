using System.Text.Json.Serialization;
namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructureTree;

/// <summary>
/// DTO for tree node representation.
/// </summary>
public class TreeNodeDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("state")]
    public TreeNodeStateDto State { get; set; } = new();

    [JsonPropertyName("data")]
    public TreeNodeDataDto? Data { get; set; }

    [JsonPropertyName("children")]
    public List<TreeNodeDto> Children { get; set; } = [];

    [JsonPropertyName("a_attr")]
    public Dictionary<string, string> Attributes { get; set; } = [];
}

/// <summary>
/// DTO for tree node state.
/// </summary>
public class TreeNodeStateDto
{
    [JsonPropertyName("opened")]
    public bool Opened { get; set; }

    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("selected")]
    public bool Selected { get; set; }
}

/// <summary>
/// DTO for tree node custom data.
/// </summary>
public class TreeNodeDataDto
{
    [JsonPropertyName("entityId")]
    public long EntityId { get; set; }

    [JsonPropertyName("sourceId")]
    public long? SourceId { get; set; }

    [JsonPropertyName("isCustomized")]
    public bool IsCustomized { get; set; }

    [JsonPropertyName("customizationStatus")]
    public string CustomizationStatus { get; set; } = "synced"; // synced, customized, custom

    // Additional properties for questions
    [JsonPropertyName("answerType")]
    public string? AnswerType { get; set; }

    [JsonPropertyName("isUsedForDiagnosis")]
    public bool? IsUsedForDiagnosis { get; set; }

    [JsonPropertyName("appliesToPhase")]
    public string? AppliesToPhase { get; set; }

    [JsonPropertyName("topicId")]
    public long? TopicId { get; set; }

    [JsonPropertyName("topicInfo")]
    public string? TopicInfo { get; set; }

    [JsonPropertyName("projectBlockId")]
    public long? ProjectBlockId { get; set; }

    // Additional properties for modules/topics
    [JsonPropertyName("order")]
    public int? Order { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    // Additional properties for answer options
    [JsonPropertyName("score")]
    public int? Score { get; set; }

    [JsonPropertyName("foda")]
    public string? Foda { get; set; }

    [JsonPropertyName("odsr")]
    public string? Odsr { get; set; }

    [JsonPropertyName("followUpQuestionText")]
    public string? FollowUpQuestionText { get; set; }

    [JsonPropertyName("fodaExplanation")]
    public string? FodaExplanation { get; set; }

    [JsonPropertyName("odsrExplanation")]
    public string? OdsrExplanation { get; set; }
}
