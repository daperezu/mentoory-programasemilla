using LinaSys.UserManagement.Application.DTOs;
using LinaSys.UserManagement.Domain.AggregatesModel.UserProfileAggregate;

namespace LinaSys.UserManagement.Application.Mappings;

public static class UserProfileMappingExtensions
{
    public static UserProfileDto ToDto(this UserProfile userProfile)
    {
        return new UserProfileDto
        {
            Id = (int)userProfile.Id,
            UserId = userProfile.UserId,
            FirstName = userProfile.FirstName,
            LastName = userProfile.LastName,
            FullName = userProfile.FullName,
            Identification = userProfile.Identification,
            Location = userProfile.Location?.ToDto(),
            AvatarUrl = userProfile.AvatarUrl,
            IsActive = userProfile.IsActive,
            Preferences = userProfile.Preferences.ToDictionary(p => p.Key, p => p.Value)
        };
    }

    public static LocationDto? ToDto(this Domain.ValueObjects.Location? location)
    {
        if (location is null)
        {
            return null;
        }

        return new LocationDto
        {
            Country = location.Country,
            Province = location.Province,
            Canton = location.Canton,
            District = location.District,
            FullAddress = location.FullAddress
        };
    }
}