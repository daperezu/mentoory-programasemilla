using System.Reflection;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class ProjectKnowledgeStructure : Entity
{
    private readonly List<ProjectModule> _projectModules = [];

    public ProjectKnowledgeStructure(
        long? sourceKnowledgeStructureId,
        long projectId,
        string name,
        bool isNameCustomized,
        string? description,
        bool isDescriptionCustomized)
    {
        SourceKnowledgeStructureId = sourceKnowledgeStructureId;
        ProjectId = projectId;
        Name = name;
        IsNameCustomized = isNameCustomized;
        Description = description;
        IsDescriptionCustomized = isDescriptionCustomized;
        IsLocked = false; // Default value
        LockedAt = null; // Default value
        LockedReason = null; // Default value
        CurrentVersion = 1; // Default version
    }

    protected ProjectKnowledgeStructure()
    {
    }

    public long? SourceKnowledgeStructureId { get; private set; }

    public long ProjectId { get; private set; }

    public string Name { get; private set; }

    public bool IsNameCustomized { get; private set; }

    public string? Description { get; private set; }

    public bool IsDescriptionCustomized { get; private set; }

    public bool IsLocked { get; private set; }

    public DateTime? LockedAt { get; private set; }

    public string? LockedReason { get; private set; }

    public int CurrentVersion { get; private set; }

    public IReadOnlyCollection<ProjectModule> ProjectModules => _projectModules.AsReadOnly();

    // Navigation property for EF Core
    internal virtual Project Project { get; private set; }

    /// <summary>
    /// Creates a ProjectKnowledgeStructure for testing purposes with an empty structure.
    /// </summary>
    public static ProjectKnowledgeStructure CreateForTesting()
    {
        return new ProjectKnowledgeStructure(
            sourceKnowledgeStructureId: null,
            projectId: 1,
            name: "Test Structure",
            isNameCustomized: false,
            description: "Test Description",
            isDescriptionCustomized: false);
    }

    /// <summary>
    /// Creates a ProjectKnowledgeStructure for testing purposes with the given questions.
    /// </summary>
    public static ProjectKnowledgeStructure CreateForTestingWithQuestions(List<ProjectQuestion> questions)
    {
        var structure = CreateForTesting();

        if (questions.Any())
        {
            var module = structure.AddProjectModule(
                sourceModuleId: 1,
                name: "Test Module",
                isNameCustomized: false,
                order: 1,
                isOrderCustomized: false);

            var topic = module.AddProjectTopic(
                sourceTopicId: 1,
                name: "Test Topic",
                isNameCustomized: false,
                order: 1,
                isOrderCustomized: false);

            // Add each question to the topic using the domain method
            foreach (var question in questions)
            {
                // Use reflection to access private fields for testing
                var topicQuestionsField = typeof(ProjectTopic).GetField("_projectQuestions", BindingFlags.NonPublic | BindingFlags.Instance);
                var questionsList = (List<ProjectQuestion>)topicQuestionsField!.GetValue(topic)!;
                questionsList.Add(question);
            }
        }

        return structure;
    }

    public void UpdateName(string name, bool isNameCustomized)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Knowledge structure name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Knowledge structure name must not exceed 100 characters.", nameof(name));
        }

        Name = name;
        IsNameCustomized = isNameCustomized;
    }

    public void UpdateDescription(string? description, bool isDescriptionCustomized)
    {
        if (description is not null && description.Length > 500)
        {
            throw new ArgumentException("Description must not exceed 500 characters.", nameof(description));
        }

        Description = description;
        IsDescriptionCustomized = isDescriptionCustomized;
    }

    public void Lock(string reason, DateTime lockedAt)
    {
        if (IsLocked)
        {
            throw new InvalidOperationException("The knowledge structure is already locked.");
        }

        IsLocked = true;
        LockedAt = lockedAt;
        LockedReason = reason;
    }

    public void Unlock()
    {
        if (!IsLocked)
        {
            throw new InvalidOperationException("The knowledge structure is not locked.");
        }

        IsLocked = false;
        LockedAt = null;
        LockedReason = null;
    }

    /// <summary>
    /// Increments the current version of the knowledge structure.
    /// This should be called when the structure is modified in a way that affects form compatibility.
    /// </summary>
    public void IncrementVersion()
    {
        if (IsLocked)
        {
            throw new InvalidOperationException("No se puede incrementar la versión de una estructura bloqueada.");
        }

        CurrentVersion++;
    }

    public ProjectModule AddProjectModule(
        long? sourceModuleId,
        string name,
        bool isNameCustomized,
        int order,
        bool isOrderCustomized)
    {
        if (_projectModules.Any(pm => pm.SourceModuleId == sourceModuleId))
        {
            throw new InvalidOperationException("A module with the same external ID already exists in this knowledge structure.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Module name cannot be null or empty.", nameof(name));
        }

        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be a non-negative integer.");
        }

        if (_projectModules.Any(pm => pm.Name == name && pm.Order == order))
        {
            throw new InvalidOperationException("A module with the same name and order already exists in this knowledge structure.");
        }

        // Ensure the project knowledge structure is not locked before adding a module
        if (IsLocked)
        {
            throw new InvalidOperationException("Cannot add a module to a locked knowledge structure.");
        }

        var projectModule = new ProjectModule(
            Id,
            sourceModuleId,
            name,
            isNameCustomized,
            order,
            isOrderCustomized);

        _projectModules.Add(projectModule);

        return projectModule;
    }

    public void RemoveProjectModule(long projectModuleId)
    {
        var projectModule = _projectModules.FirstOrDefault(pm => pm.Id == projectModuleId);

        if (projectModule is not null)
        {
            _projectModules.Remove(projectModule);
        }
        else
        {
            throw new InvalidOperationException("The specified module does not exist in this knowledge structure.");
        }
    }

    public void RemoveProjectModule(ProjectModule projectModule)
    {
        if (projectModule is null)
        {
            throw new ArgumentNullException(nameof(projectModule));
        }

        if (!_projectModules.Contains(projectModule))
        {
            throw new InvalidOperationException("The specified module does not exist in this knowledge structure.");
        }

        _projectModules.Remove(projectModule);
    }

    /// <summary>
    /// Finds a topic by its source ID across all modules.
    /// </summary>
    /// <param name="sourceTopicId">The source topic ID to find.</param>
    /// <returns>The project topic if found, null otherwise.</returns>
    public ProjectTopic? FindTopicBySourceId(long sourceTopicId)
    {
        return _projectModules
            .SelectMany(m => m.ProjectTopics)
            .FirstOrDefault(t => t.SourceTopicId == sourceTopicId);
    }
}