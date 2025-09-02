using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Core.Application.Dashboard.Queries.GetDashboard;

public record GetDashboardQuery(string UserId, string Role) : IBaseRequest<DashboardDto>;
