using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class ProjectSubjectResource : Entity
{
    public ProjectSubjectResource(
        long projectSubjectId,
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
        ProjectSubjectId = projectSubjectId;
        SourceSubjectResourceId = sourceSubjectResourceId;
        Title = title;
        IsTitleCustomized = isTitleCustomized;
        Url = url;
        IsUrlCustomized = isUrlCustomized;
        Type = type;
        IsTypeCustomized = isTypeCustomized;
        EstimatedMinutes = estimatedMinutes;
        IsEstimatedMinutesCustomized = isEstimatedMinutesCustomized;
        Order = order;
        IsOrderCustomized = isOrderCustomized;
    }

    protected ProjectSubjectResource()
    {
    }

    public long ProjectSubjectId { get; private set; }

    public long? SourceSubjectResourceId { get; private set; }

    public string Title { get; private set; }

    public bool IsTitleCustomized { get; private set; }

    public string Url { get; private set; }

    public bool IsUrlCustomized { get; private set; }

    public string Type { get; private set; }

    public bool IsTypeCustomized { get; private set; }

    public int? EstimatedMinutes { get; private set; }

    public bool IsEstimatedMinutesCustomized { get; private set; }

    public int Order { get; private set; }

    public bool IsOrderCustomized { get; private set; }

    public virtual ProjectSubject ProjectSubject { get; private set; }

    public void UpdateTitle(string title, bool isCustomized)
    {
        Title = title;
        IsTitleCustomized = isCustomized;
    }

    public void UpdateUrl(string url, bool isCustomized)
    {
        Url = url;
        IsUrlCustomized = isCustomized;
    }

    public void UpdateType(string type, bool isCustomized)
    {
        Type = type;
        IsTypeCustomized = isCustomized;
    }

    public void UpdateEstimatedMinutes(int? estimatedMinutes, bool isCustomized)
    {
        EstimatedMinutes = estimatedMinutes;
        IsEstimatedMinutesCustomized = isCustomized;
    }

    public void UpdateOrder(int order, bool isCustomized)
    {
        Order = order;
        IsOrderCustomized = isCustomized;
    }
}
