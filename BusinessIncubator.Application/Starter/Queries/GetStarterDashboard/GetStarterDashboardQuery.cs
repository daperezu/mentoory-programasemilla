using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Starter.Queries.GetStarterDashboard;

public class GetStarterDashboardQuery(string userId, long projectId) : IBaseRequest<StarterDashboardDto>
{
    public string UserId { get; } = userId;

    public long ProjectId { get; } = projectId;
}
