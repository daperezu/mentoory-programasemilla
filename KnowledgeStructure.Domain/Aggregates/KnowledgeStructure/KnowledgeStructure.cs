using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;

public class KnowledgeStructure : Entity, IAggregateRoot
{
    private readonly List<KnowledgeStructureModule> _knowledgeStructureModules = [];

    public KnowledgeStructure(string name, string? description, bool isActive, DateTime createdAt)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    private KnowledgeStructure()
    {
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<KnowledgeStructureModule> KnowledgeStructureModules => _knowledgeStructureModules.AsReadOnly();

    public void AddModule(Module.Module module, int order)
    {
        var structureModule = new KnowledgeStructureModule(this, module, order);
        _knowledgeStructureModules.Add(structureModule);
    }

    public void Deactivate(string reason)
    {
        IsActive = false;
    }

    public void RemoveModule(long moduleId)
    {
        var module = _knowledgeStructureModules.FirstOrDefault(f => f.Id == moduleId);
        if (module is not null)
        {
            _knowledgeStructureModules.Remove(module);
        }
    }

    public void Rename(string newName) => Name = newName;

    public void UpdateDescription(string? description) => Description = description;

    public void Activate() => IsActive = true;
}
