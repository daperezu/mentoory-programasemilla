using LinaSys.Diagnostics.Domain.Aggregates.Form;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Aggregates.Block;

public class Block : Entity, IAggregateRoot
{
    public Block(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Block name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    private Block()
    {
    }

    public string Name { get; private set; }

    // Navigation property for EF Core - not part of the aggregate
    // FormQuestions belong to Form aggregate, Block only holds reference
    internal virtual ICollection<FormQuestion> FormQuestions { get; private set; } = [];

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("New block name cannot be empty.", nameof(newName));
        }

        Name = newName.Trim();
    }

    // Domain method to check if block can be deleted
    // This requires a domain service or repository check, not direct navigation
    public bool CanBeDeleted()
    {
        // The actual check should be done via repository/domain service
        // to respect aggregate boundaries
        return true;
    }
}
