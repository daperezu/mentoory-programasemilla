using MediatR;

namespace LinaSys.UserManagement.Domain.Events;

public class UserProfileCreatedDomainEvent(
    int userProfileId,
    string userId,
    string firstName,
    string lastName,
    string identification) : INotification
{
    public int UserProfileId { get; } = userProfileId;
    public string UserId { get; } = userId;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string Identification { get; } = identification;
}