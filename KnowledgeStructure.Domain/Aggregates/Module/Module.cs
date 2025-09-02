using LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.KnowledgeStructure.Domain.Aggregates.Module;

public class Module : Entity, IAggregateRoot
{
    private readonly List<KnowledgeStructureModule> _knowledgeStructureModules = [];

    public Module(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Module name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    private Module()
    {
    }

    public string Name { get; private set; }

    public IReadOnlyCollection<KnowledgeStructureModule> KnowledgeStructureModules => _knowledgeStructureModules.AsReadOnly();

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("New module name cannot be empty.", nameof(newName));
        }

        Name = newName.Trim();
    }
}
