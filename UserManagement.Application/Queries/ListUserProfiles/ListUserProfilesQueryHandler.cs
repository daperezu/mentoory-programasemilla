using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.UserManagement.Application.DTOs;
using LinaSys.UserManagement.Application.Mappings;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Queries.ListUserProfiles;

public class ListUserProfilesQueryHandler(
    IUserProfileRepository userProfileRepository,
    ILogger<ListUserProfilesQueryHandler> logger)
    : BaseCommandHandler<ListUserProfilesQuery, FilteredQueryResult<UserProfileDto>>
{
    public override async Task<Result<FilteredQueryResult<UserProfileDto>>> Handle(
        ListUserProfilesQuery request,
        CancellationToken cancellationToken)
    {
        // For now, return all active profiles - in a real implementation,
        // we'd add proper filtering and pagination methods to the repository
        var profiles = await userProfileRepository.GetActiveProfilesAsync();
        var profilesList = profiles.ToList();

        // Apply filters in memory (not optimal, but works for now)
        var filteredProfiles = profilesList.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            filteredProfiles = filteredProfiles.Where(p =>
                p.FirstName.ToLower().Contains(searchTerm) ||
                p.LastName.ToLower().Contains(searchTerm) ||
                p.Identification.ToLower().Contains(searchTerm) ||
                p.UserId.ToLower().Contains(searchTerm));
        }

        var filteredList = filteredProfiles.ToList();
        var recordsTotal = profilesList.Count;
        var recordsFiltered = filteredList.Count;

        // Apply pagination
        var paginatedProfiles = filteredList
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Skip(request.Start)
            .Take(request.Length)
            .ToList();

        // Map to DTOs
        var items = paginatedProfiles.Select(p => p.ToDto()).ToList();

        var result = FilteredQueryResult.From(items, recordsTotal, recordsFiltered);

        logger.LogInformation(
            "Retrieved {Count} user profiles (total: {Total}, filtered: {Filtered})",
            items.Count,
            recordsTotal,
            recordsFiltered);

        return Success(result);
    }
}

