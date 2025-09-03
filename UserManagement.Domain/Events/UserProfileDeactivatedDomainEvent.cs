using MediatR;

namespace LinaSys.UserManagement.Domain.Events;

public class UserProfileDeactivatedDomainEvent(int userProfileId, string userId) : INotification
{
    public int UserProfileId { get; } = userProfileId;
    public string UserId { get; } = userId;
}