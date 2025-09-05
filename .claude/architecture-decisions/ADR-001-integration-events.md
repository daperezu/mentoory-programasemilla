# ADR-001: Integration Events in Modular Monolith

## Status
Accepted

## Context
LinaSys is built as a modular monolith with the intention of potentially extracting modules into microservices in the future. We need to establish communication patterns between bounded contexts (domains) that:
- Maintain loose coupling between modules
- Enable easy extraction to microservices
- Keep the codebase simple and maintainable
- Avoid over-engineering for a future that might not come

## Decision
We will allow direct references between domains **ONLY for integration events**, accepting this minimal coupling as a pragmatic trade-off in our modular monolith architecture.

### Pattern
```csharp
// BusinessIncubator.Application references Permissions.Application.IntegrationEvents
using LinaSys.Permissions.Application.IntegrationEvents;
using LinaSys.Permissions.Domain.Constants; // For ResourceTypes

// Publish event directly
await integrationEventService.PublishAsync(
    new ProtectedResourceCreated(id, ResourceTypes.BusinessIncubator, name, user)
);
```

## Consequences

### Positive
- **Simple and direct**: Easy to understand event flow
- **Traceable**: Can easily find all cross-domain dependencies
- **Type-safe**: Compile-time checking of event contracts
- **Migration-ready**: Clear boundaries for future extraction
- **Minimal coupling**: Only event contracts are shared, no business logic

### Negative
- **Cross-domain references**: Violates pure DDD boundaries
- **Compile-time coupling**: Changes to events require recompilation
- **Potential for abuse**: Developers might reference more than just events

### Mitigation Strategies
1. **Strict code review**: Ensure only integration events are referenced
2. **Clear naming**: Use `IntegrationEvents` namespace/suffix
3. **Documentation**: Mark these dependencies as integration contracts
4. **Architectural tests**: Automated checks for unwanted references

## Alternatives Considered

### 1. Shared Event Library
Place all integration events in `LinaSys.Shared.Application.IntegrationEvents`
- ❌ Creates a "god library" that grows indefinitely
- ❌ Less clear ownership of events
- ❌ Harder to extract single modules

### 2. Event Translation Layer
Use Orchestration layer to translate between domain-specific events
- ❌ Adds unnecessary complexity
- ❌ More code to maintain
- ❌ Indirect event flow harder to debug

### 3. Pure Decoupling (String-based events)
Use weakly-typed events with string identifiers
- ❌ Loss of type safety
- ❌ Runtime errors instead of compile-time
- ❌ Harder refactoring

## Migration Path to Microservices

When extracting a module to a microservice:

1. **Copy event definitions** to the new service
2. **Replace direct publishing** with message bus (RabbitMQ/Kafka/ServiceBus)
3. **Version events** for backward compatibility
4. **Optional: Use schema registry** for contract management

Example transformation:
```csharp
// Monolith
await integrationEventService.PublishAsync(new ProtectedResourceCreated(...));

// Microservice
await messageBus.PublishAsync("protected-resource-created", new {
    Id = ...,
    ResourceType = "BusinessIncubator",
    Name = ...,
    CreatedBy = ...
});
```

## Examples in Our Codebase

### Current Implementation
- `BusinessIncubator.Application` → `Permissions.Application.IntegrationEvents`
  - Uses: `ProtectedResourceCreated` event
  - Uses: `ResourceTypes` constants
  
- `Project.Application` → `Permissions.Application.IntegrationEvents`
  - Uses: `ProtectedResourceCreated` event
  - Uses: `ResourceTypes` constants

### Guidelines
1. **ONLY reference integration events and related constants**
2. **NEVER reference domain entities, aggregates, or services**
3. **Document the dependency** in module documentation
4. **Keep events immutable** and simple (data contracts only)

## References
- [Modular Monolith with DDD](https://github.com/kgrzybek/modular-monolith-with-ddd)
- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [Martin Fowler - Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html)

## Decision Date
2025-01-10

## Participants
- Development Team
- Architecture Review

## Notes
This decision prioritizes pragmatism and simplicity over theoretical purity. We acknowledge the trade-offs and accept them as appropriate for our current stage and future plans.