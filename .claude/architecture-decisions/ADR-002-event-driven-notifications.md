# ADR-002: Event-Driven Notifications Architecture

## Status
Implemented

## Context
The system initially had email sending logic scattered across multiple domains and layers:
- Direct `SendEmailCommand` calls in domain command handlers
- Email preference checking duplicated in multiple places
- Orchestration layer handling email preferences
- Tight coupling between business logic and notification concerns

This created several problems:
- Violation of Single Responsibility Principle
- Difficult to maintain and test
- Inconsistent preference checking
- Performance issues with synchronous email sending

## Decision
We have implemented an event-driven architecture for all notifications where:

1. **Domain commands publish integration events** after successful operations
2. **Notification domain handles all email logic** via event handlers
3. **Centralized preference checking** in the Notification domain
4. **Asynchronous processing** via MediatR's publish-subscribe pattern

## Implementation

### Integration Events
Each domain publishes specific integration events:

**Auth Domain:**
- `UserAccountCreatedIntegrationEvent` - When a new user account is created
- `PasswordResetRequestedIntegrationEvent` - When password reset is requested
- `EmailChangeRequestedIntegrationEvent` - When email change is requested

**BusinessIncubator Domain:**
- `FormSubmittedIntegrationEvent` - When a form is submitted
- `FormApprovedIntegrationEvent` - When a form is approved
- `FormRejectedIntegrationEvent` - When a form is rejected
- `UserInvitedToProjectIntegrationEvent` - When user is invited to project
- `ReviewChangesRequestedIntegrationEvent` - When changes are requested

**UserManagement Domain:**
- `UserProfileCreatedIntegrationEvent` - When user profile is created
- `UserProfileUpdatedIntegrationEvent` - When user profile is updated

### Event Handlers
Each integration event has a corresponding handler in `Notification.Application.IntegrationEventHandlers`:

```csharp
public class FormApprovedHandler : NotificationEventHandler<FormApprovedIntegrationEvent>
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(
        FormApprovedIntegrationEvent notification)
    {
        return (notification.ParticipantUserId, "email.approvals");
    }

    protected override async Task ProcessNotificationAsync(
        FormApprovedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Generate and send email
    }
}
```

### Base Handler Pattern
All notification handlers extend `NotificationEventHandler<T>` which provides:
- Automatic preference checking before sending
- Consistent error handling
- Logging
- Non-blocking execution (errors don't fail business operations)

### Preference System
Email preferences are checked via `IEmailPreferenceService`:
- Queries UserManagement domain for user preferences
- Supports multiple preference keys (email.approvals, email.rejections, etc.)
- Provides sensible defaults for critical emails

## Consequences

### Positive
- **Clean separation of concerns** - Business logic doesn't know about emails
- **Scalability** - Email sending is asynchronous and can be scaled independently
- **Maintainability** - All email logic in one place
- **Flexibility** - Easy to add new notification channels (SMS, push, etc.)
- **Testability** - Business logic can be tested without email infrastructure
- **Performance** - Operations complete faster without waiting for email sending

### Negative
- **Eventual consistency** - Emails may be sent after the operation completes
- **Complexity** - More moving parts with event publishing and handling
- **Debugging** - Tracing issues across event boundaries can be harder

### Neutral
- **Learning curve** - Developers need to understand event-driven patterns
- **More files** - Each event and handler is a separate file

## Migration Notes

### Removed Components
- `SendFormApprovalEmailCommand` - Obsolete orchestration command
- `SendWelcomeEmailOrchestrationCommand` - Obsolete orchestration command
- `SendEmailWithPreferencesCommand` - Obsolete orchestration command
- `FormApprovalNotificationRequested` - Old event type
- `IEmailPreferenceChecker` - Moved to Notification domain

### Updated Components
- All domain command handlers now publish events instead of sending emails
- Notification infrastructure auto-registers all event handlers via MediatR

### Remaining Direct Email Usage
- `Web/Areas/Identity/Pages/Account/Register.cshtml.cs` - Email confirmation requires special handling with tokens
- These may be addressed in a future iteration with dedicated confirmation events

## Related
- ADR-001: Integration Events for Cross-Domain Communication
- Clean Architecture principles
- Domain-Driven Design bounded contexts