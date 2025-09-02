using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class ProjectModule : Entity
{
    private readonly List<ProjectTopic> _projectTopics = [];

    public ProjectModule(
        long projectKnowledgeStructureId,
        long? sourceModuleId,
        string name,
        bool isNameCustomized,
        int order,
        bool isOrderCustomized)
    {
        ProjectKnowledgeStructureId = projectKnowledgeStructureId;
        SourceModuleId = sourceModuleId;
        Name = name;
        IsNameCustomized = isNameCustomized;
        Order = order;
        IsOrderCustomized = isOrderCustomized;
    }

    protected ProjectModule()
    {
    }

    public long ProjectKnowledgeStructureId { get; private set; }

    public long? SourceModuleId { get; private set; }

    public string Name { get; private set; }

    public bool IsNameCustomized { get; private set; }

    public int Order { get; private set; }

    public bool IsOrderCustomized { get; private set; }

    public IReadOnlyCollection<ProjectTopic> ProjectTopics => _projectTopics.AsReadOnly();

    // Navigation property for EF Core
    internal virtual ProjectKnowledgeStructure ProjectKnowledgeStructure { get; private set; }

    public void UpdateName(string name, bool isNameCustomized)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Module name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Module name must not exceed 100 characters.", nameof(name));
        }

        if (ProjectKnowledgeStructure.ProjectModules.Any(pm => pm.Name == name && pm.Id != Id))
        {
            throw new InvalidOperationException("A module with the same name already exists in this knowledge structure.");
        }

        Name = name;
        IsNameCustomized = isNameCustomized;
    }

    public void UpdateOrder(int order, bool isOrderCustomized)
    {
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be a non-negative integer.");
        }

        Order = order;
        IsOrderCustomized = isOrderCustomized;
    }

    public ProjectTopic AddProjectTopic(
        long? sourceTopicId,
        string name,
        bool isNameCustomized,
        int order,
        bool isOrderCustomized)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project topic name cannot be null or empty.", nameof(name));
        }

        if (sourceTopicId is not null && _projectTopics.Any(pt => pt.SourceTopicId == sourceTopicId))
        {
            throw new InvalidOperationException("A topic with the same external ID already exists in this module.");
        }

        if (_projectTopics.Any(pt => pt.Name == name && pt.Order == order))
        {
            throw new InvalidOperationException("A topic with the same name and order already exists in this module.");
        }

        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be a non-negative integer.");
        }

        var projectTopic = new ProjectTopic(
            Id,
            sourceTopicId,
            name,
            isNameCustomized,
            order,
            isOrderCustomized);

        _projectTopics.Add(projectTopic);

        return projectTopic;
    }

    public void RemoveProjectTopic(long projectTopicId)
    {
        var projectTopic = _projectTopics.FirstOrDefault(pt => pt.Id == projectTopicId);

        if (projectTopic is not null)
        {
            _projectTopics.Remove(projectTopic);
        }
        else
        {
            throw new InvalidOperationException("Project topic not found.");
        }
    }

    public void ClearSource()
    {
        SourceModuleId = null;
    }

    public void CustomizeName(string name)
    {
        UpdateName(name, isNameCustomized: true);
    }

    public void ResetNameToSource(string sourceName)
    {
        UpdateName(sourceName, isNameCustomized: false);
    }

    public void CustomizeOrder(int order)
    {
        UpdateOrder(order, isOrderCustomized: true);
    }

    public void ResetOrderToSource(int sourceOrder)
    {
        UpdateOrder(sourceOrder, isOrderCustomized: false);
    }

    public bool IsFullyCustomized() =>
        IsNameCustomized && IsOrderCustomized;
}
