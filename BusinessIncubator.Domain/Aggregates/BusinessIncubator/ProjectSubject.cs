using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class ProjectSubject : Entity
{
    private readonly List<ProjectSubjectResource> _projectSubjectResources = [];
    private readonly List<ProjectAnswerOption> _projectAnswerOptions = [];

    public ProjectSubject(
        long projectTopicId,
        long? sourceSubjectId,
        string title,
        bool isTitleCustomized,
        string? content,
        bool isContentCustomized,
        int order,
        bool isOrderCustomized)
    {
        ProjectTopicId = projectTopicId;
        SourceSubjectId = sourceSubjectId;
        Title = title;
        IsTitleCustomized = isTitleCustomized;
        Content = content;
        IsContentCustomized = isContentCustomized;
        Order = order;
        IsOrderCustomized = isOrderCustomized;
    }

    protected ProjectSubject()
    {
    }

    public long ProjectTopicId { get; private set; }

    public long? SourceSubjectId { get; private set; }

    public string Title { get; private set; }

    public bool IsTitleCustomized { get; private set; }

    public string? Content { get; private set; }

    public bool IsContentCustomized { get; private set; }

    public int Order { get; private set; }

    public bool IsOrderCustomized { get; private set; }

    public IReadOnlyCollection<ProjectSubjectResource> ProjectSubjectResources => _projectSubjectResources.AsReadOnly();

    public IReadOnlyCollection<ProjectAnswerOption> ProjectAnswerOptions => _projectAnswerOptions.AsReadOnly();

    // Navigation property for EF Core
    internal virtual ProjectTopic ProjectTopic { get; private set; }

    public void UpdateTitle(string title, bool isCustomized)
    {
        Title = title;
        IsTitleCustomized = isCustomized;
    }

    public void UpdateContent(string content, bool isCustomized)
    {
        Content = content;
        IsContentCustomized = isCustomized;
    }

    public void UpdateOrder(int order, bool isCustomized)
    {
        Order = order;
        IsOrderCustomized = isCustomized;
    }

    public ProjectSubjectResource AddProjectSubjectResource(
        long? sourceSubjectResourceId,
        string title,
        bool isTitleCustomized,
        string url,
        bool isUrlCustomized,
        string type,
        bool isTypeCustomized,
        int? estimatedMinutes,
        bool isEstimatedMinutesCustomized,
        int order,
        bool isOrderCustomized)
    {
        var resource = new ProjectSubjectResource(
            Id,
            sourceSubjectResourceId,
            title,
            isTitleCustomized,
            url,
            isUrlCustomized,
            type,
            isTypeCustomized,
            estimatedMinutes,
            isEstimatedMinutesCustomized,
            order,
            isOrderCustomized);

        _projectSubjectResources.Add(resource);
        return resource;
    }

    public void ClearSource()
    {
        SourceSubjectId = null;
    }

    public void CustomizeTitle(string title)
    {
        UpdateTitle(title, isCustomized: true);
    }

    public void ResetTitleToSource(string sourceTitle)
    {
        UpdateTitle(sourceTitle, isCustomized: false);
    }

    public void CustomizeContent(string content)
    {
        UpdateContent(content, isCustomized: true);
    }

    public void ResetContentToSource(string sourceContent)
    {
        UpdateContent(sourceContent, isCustomized: false);
    }

    public void CustomizeOrder(int order)
    {
        UpdateOrder(order, isCustomized: true);
    }

    public void ResetOrderToSource(int sourceOrder)
    {
        UpdateOrder(sourceOrder, isCustomized: false);
    }

    public bool IsFullyCustomized() =>
        IsTitleCustomized && IsContentCustomized && IsOrderCustomized;
}
