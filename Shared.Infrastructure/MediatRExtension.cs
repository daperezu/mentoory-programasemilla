using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Shared.Infrastructure;

public static class MediatRExtension
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, DbContext context)
    {
        var domainEntities = context.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents is not null && x.Entity.DomainEvents.Count != 0)
            .ToList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEntities.SelectMany(x => x.Entity.DomainEvents!))
        {
            await mediator.Publish(domainEvent);
        }
    }
}
