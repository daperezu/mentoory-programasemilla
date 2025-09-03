using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Web.Infrastructure.Services;

public record AuditContext(DateTime UtcNow, string? User) : IAuditContext;
