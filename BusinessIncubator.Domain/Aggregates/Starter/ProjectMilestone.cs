using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.Starter;

public class ProjectMilestone : Entity
{
    public ProjectMilestone(
        long projectId,
        string name,
        string description,
        DateTime targetDate,
        string? requirements = null,
        int orderIndex = 0)
        : this()
    {
        ProjectId = projectId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        TargetDate = targetDate;
        Requirements = requirements;
        OrderIndex = orderIndex;
    }

    protected ProjectMilestone()
    {
        Name = string.Empty;
        Description = string.Empty;
        Status = "pending";
        Progress = 0;
        OrderIndex = 0;
        CreatedDate = DateTime.UtcNow;
    }

    public long ProjectId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime TargetDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string Status { get; private set; }
    public decimal Progress { get; private set; }
    public string? Requirements { get; private set; }
    public int OrderIndex { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }

    public void UpdateProgress(decimal progress)
    {
        if (progress < 0 || progress > 100)
        {
            throw new ArgumentException("El progreso debe estar entre 0 y 100", nameof(progress));
        }

        Progress = progress;

        if (progress == 100 && Status != "completed")
        {
            Complete();
        }
        else if (progress > 0 && progress < 100)
        {
            Status = "in_progress";
        }

        ModifiedDate = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = "completed";
        CompletedDate = DateTime.UtcNow;
        Progress = 100;
        ModifiedDate = DateTime.UtcNow;
    }

    public void UpdateTargetDate(DateTime targetDate)
    {
        TargetDate = targetDate;
        ModifiedDate = DateTime.UtcNow;
    }

    public List<string> GetRequirementsList()
    {
        if (string.IsNullOrWhiteSpace(Requirements))
        {
            return [];
        }

        return Requirements.Split('|', StringSplitOptions.RemoveEmptyEntries)
                          .Select(r => r.Trim())
                          .ToList();
    }

    public bool IsOverdue()
    {
        return Status != "completed" && TargetDate < DateTime.UtcNow;
    }
}