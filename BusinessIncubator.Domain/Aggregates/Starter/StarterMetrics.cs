using LinaSys.Core.Domain.Aggregates.Dashboard;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.Starter;

public class StarterMetrics(
    decimal overallProgress,
    int pendingItems,
    int completedItems,
    int totalItems,
    int unreadNotifications,
    DateTime? lastActivityDate,
    int daysSinceStart,
    string currentPhase,
    int formsCompleted,
    int formsTotal,
    int overdueTasks) : DashboardMetrics(overallProgress, pendingItems, completedItems, totalItems,
           unreadNotifications, lastActivityDate, daysSinceStart, currentPhase)
{
    public int FormsCompleted { get; private set; } = formsCompleted >= 0 ? formsCompleted : 0;
    public int FormsTotal { get; private set; } = formsTotal >= 0 ? formsTotal : 0;
    public int OverdueTasks { get; private set; } = overdueTasks >= 0 ? overdueTasks : 0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var component in base.GetEqualityComponents())
        {
            yield return component;
        }

        yield return FormsCompleted;
        yield return FormsTotal;
        yield return OverdueTasks;
    }
}