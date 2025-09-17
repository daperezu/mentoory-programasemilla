using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.BusinessIncubator.Infrastructure.Persistence.Repositories;

public class BusinessIncubatorRepository(BusinessIncubatorDbContext dbContext)
    : AbstractRepository<Domain.Aggregates.BusinessIncubator.BusinessIncubator>(dbContext), IBusinessIncubatorRepository
{
    /// <inheritdoc/>
    public async Task AddProjectAsync(Project project, CancellationToken cancellationToken = default)
    {
        await dbContext.Projects.AddAsync(project, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddReviewAsync(ProjectFormReview review, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<ProjectFormReview>().AddAsync(review, cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken)
    {
        return dbContext.BusinessIncubators
            .AnyAsync(f => f.Key == key, cancellationToken);
    }

    public Task<bool> ExistsByKeyNotItselfAsync(Guid externalId, string key, CancellationToken cancellationToken)
    {
        return dbContext.BusinessIncubators
            .AnyAsync(f => f.ExternalId != externalId && f.Key == key, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return dbContext.BusinessIncubators
            .AnyAsync(f => f.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameNotItselfAsync(Guid externalId, string name, CancellationToken cancellationToken)
    {
        return dbContext.BusinessIncubators
            .AnyAsync(f => f.ExternalId != externalId && f.Name == name, cancellationToken);
    }

    public Task<bool> ExistsProjectByExternalIdAsync(Guid businessIncubatorExternalId, Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        return dbContext.BusinessIncubators
            .AsNoTracking()
            .Where(bi => bi.ExternalId == businessIncubatorExternalId)
            .SelectMany(bi => dbContext.Projects.Where(p => p.BusinessIncubatorId == bi.Id))
            .AnyAsync(p => p.ExternalId == projectExternalId, cancellationToken);
    }

    public Task<List<Domain.Aggregates.BusinessIncubator.BusinessIncubator>> GetAllIncubators(CancellationToken cancellationToken = default)
    {
        return dbContext.BusinessIncubators.ToListAsync(cancellationToken);
    }

    public async Task<List<Project>> GetAllProjectsWithInvitationsAsync(CancellationToken cancellationToken = default)
    {
        var projects = await dbContext.Projects
            .Include("ProjectInvitations")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return projects;
    }

    /// <inheritdoc/>
    public async Task<BatchUserRegistration?> GetBatchRegistrationByExternalIdAsync(Guid batchExternalId, CancellationToken cancellationToken = default)
    {
        var batchRegistration = await dbContext.BatchUserRegistrations
            .AsNoTracking()
            .Include("Project")
            .Where(b => b.ExternalId == batchExternalId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return batchRegistration;
    }

    public async Task<Domain.Aggregates.BusinessIncubator.BusinessIncubator?> GetBusinessIncubatorByExternalIdAsync(Guid externalId)
    {
        return await GetByExternalIdAsync(externalId, CancellationToken.None);
    }

    public Task<Domain.Aggregates.BusinessIncubator.BusinessIncubator?> GetByExternalIdAsync(Guid externalId, CancellationToken cancellationToken)
    {
        return dbContext.BusinessIncubators
            .Where(f => f.ExternalId == externalId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Domain.Aggregates.BusinessIncubator.BusinessIncubator?> GetByExternalIdIncludingDeletedAsync(Guid externalId, CancellationToken cancellationToken = default)
    {
        return dbContext.BusinessIncubators
            .IgnoreQueryFilters()
            .Where(f => f.ExternalId == externalId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Domain.Aggregates.BusinessIncubator.BusinessIncubator?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return dbContext.BusinessIncubators
            .Where(f => f.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Domain.Aggregates.BusinessIncubator.BusinessIncubator>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        return await dbContext.BusinessIncubators
            .Where(bi => ids.Contains(bi.Id) && !bi.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Domain.Aggregates.BusinessIncubator.BusinessIncubator?> GetByProjectExternalIdWithQuestionsAsync(Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        // First get the project to find the business incubator ID
        var project = await dbContext.Projects
            .Where(p => p.ExternalId == projectExternalId)
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            return null;
        }

        // Then get the business incubator
        var businessIncubator = await dbContext.BusinessIncubators
            .Where(bi => bi.Id == project.BusinessIncubatorId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        // Load related data separately if needed
        if (businessIncubator is not null)
        {
            // Load projects with all related data
            await dbContext.Entry(businessIncubator)
                .Collection("Projects")
                .Query()
                .Cast<Project>()
                .Where(p => p.ExternalId == projectExternalId)
                .Include("ProjectBlocks")
                .Include("BatchUserRegistrations")
                .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectQuestions.ProjectAnswerOptions")
                .LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        return businessIncubator;
    }

    /// <inheritdoc/>
    public async Task<Domain.Aggregates.BusinessIncubator.BusinessIncubator?> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        // First find the project to get the business incubator ID
        var project = await dbContext.Projects
            .Where(p => p.Id == projectId)
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            return null;
        }

        // Then get the business incubator
        var businessIncubator = await dbContext.BusinessIncubators
            .AsNoTracking()
            .Where(bi => bi.Id == project.BusinessIncubatorId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return businessIncubator;
    }

    /// <inheritdoc/>
    public async Task<ProjectFormReview?> GetLatestReviewBySubmissionIdAsync(long submissionId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormReview>()
            .Include(r => r.FeedbackItems)
            .Where(r => r.SubmissionId == submissionId)
            .OrderByDescending(r => r.ReviewedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<int> GetPendingSubmissionsCountAsync(long[] projectIds, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormSubmission>()
            .Where(s => projectIds.Contains(s.ProjectId) &&
                s.Status == ProjectFormSubmissionStatus.Submitted)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ProjectFormSubmission>> GetPendingSubmissionsForReviewAsync(long[] projectIds, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await dbContext.Set<ProjectFormSubmission>()
            .Where(s => projectIds.Contains(s.ProjectId) &&
                s.Status == ProjectFormSubmissionStatus.Submitted)
            .OrderBy(s => s.SubmittedAt ?? s.StartedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<ProjectAnswerOptionReferenceDto>> GetProjectAnswerOptionReferencesBySourceIdAsync(long sourceAnswerOptionId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectAnswerOption>()
            .Where(pao => pao.SourceAnswerOptionId == sourceAnswerOptionId)
            .Select(pao => new ProjectAnswerOptionReferenceDto(pao.ProjectQuestion.ProjectBlock.ProjectId, pao.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<(HashSet<string> Names, HashSet<long> SourceIds)> GetProjectBlockIdentifiersAsync(long projectId, CancellationToken cancellationToken = default)
    {
        var blocks = await dbContext.ProjectBlocks
            .Where(pb => pb.ProjectId == projectId)
            .Select(pb => new { pb.Name, pb.SourceBlockId })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var names = new HashSet<string>(blocks.Select(b => b.Name));
        var sourceIds = new HashSet<long>(blocks.Where(b => b.SourceBlockId.HasValue).Select(b => b.SourceBlockId!.Value));

        return (names, sourceIds);
    }

    public async Task<Project?> GetProjectByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects
            .Include("ProjectInvitations")
            .Include("BatchUserRegistrations")
            .Include("ProjectBlocks.ProjectQuestions.ProjectAnswerOptions")
            .FirstOrDefaultAsync(p => p.ExternalId == projectExternalId, cancellationToken)
            .ConfigureAwait(false);

        return project;
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectByExternalIdIncludingDeletedAsync(Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ExternalId == projectExternalId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectByIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ProjectDiagnosisBlockDto>> GetProjectDiagnosisQuestionsAsync(Guid projectExternalId, int questionPhase, CancellationToken cancellationToken = default)
    {
        var questionPhaseEnum = (QuestionPhase)questionPhase;

        var query = from project in dbContext.Projects
                    join block in dbContext.ProjectBlocks on project.Id equals block.ProjectId
                    join question in dbContext.ProjectQuestions on block.Id equals question.ProjectBlockId
                    where project.ExternalId == projectExternalId
                          && question.IsUsedForDiagnosis
                          && (question.AppliesToPhase == questionPhaseEnum || question.AppliesToPhase == QuestionPhase.Both)
                    orderby block.Id, question.Order
                    select new
                    {
                        BlockId = block.Id,
                        BlockTitle = block.Name,
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        QuestionIsTextCustomized = question.IsTextCustomized,
                        QuestionAnswerType = (int)question.AnswerType,
                        QuestionOrder = question.Order,
                    };

        var questionData = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        if (!questionData.Any())
        {
            return [];
        }

        // Get answer options for all questions
        var questionIds = questionData.Select(q => q.QuestionId).Distinct().ToList();
        var answerOptions = await dbContext.ProjectAnswerOptions
            .Where(ao => questionIds.Contains(ao.ProjectQuestionId))
            .OrderBy(ao => ao.Order)
            .Select(ao => new
            {
                ao.Id,
                ao.ProjectQuestionId,
                ao.Text,
                ao.FollowUpQuestionText,
                ao.IsTextCustomized,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Group by block and build the result
        var blocks = questionData
            .GroupBy(q => new { q.BlockId, q.BlockTitle })
            .Select(blockGroup => new ProjectDiagnosisBlockDto(
                blockGroup.Key.BlockId,
                blockGroup.Key.BlockTitle,
                blockGroup.Select(q => new ProjectDiagnosisQuestionDto(
                    q.QuestionId,
                    q.QuestionAnswerType,
                    q.QuestionText,
                    q.QuestionIsTextCustomized,
                    answerOptions
                        .Where(ao => ao.ProjectQuestionId == q.QuestionId)
                        .Select(ao => new ProjectDiagnosisAnswerOptionDto(
                            ao.Id,
                            ao.Text,
                            ao.FollowUpQuestionText,
                            ao.IsTextCustomized))
                        .ToList()))
                .ToList()))
            .ToList();

        return blocks;
    }

    /// <inheritdoc/>
    public async Task<List<ProjectFormSubmission>> GetProjectFormSubmissionsByUserAsync(long projectId, string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProjectFormSubmissions
            .Where(x => x.ProjectId == projectId && x.ParticipantUserId == userId)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<long>> GetProjectIdsBySourceFormIdAsync(long sourceFormId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Project>()
            .Where(p => p.SourceFormId == sourceFormId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<ProjectInvitation?> GetProjectInvitationByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var invitation = await dbContext.ProjectInvitations
            .Include("Project")
            .Where(i => i.InvitationToken == token)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return invitation;
    }

    public async Task<ProjectKnowledgeStructure?> GetProjectKnowledgeStructureAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectKnowledgeStructure>()
            .Include(pks => pks.ProjectModules)
                .ThenInclude(m => m.ProjectTopics)
                    .ThenInclude(t => t.ProjectQuestions)
                        .ThenInclude(q => q.ProjectAnswerOptions)
            .Include(pks => pks.ProjectModules)
                .ThenInclude(m => m.ProjectTopics)
                    .ThenInclude(t => t.ProjectSubjects)
                        .ThenInclude(s => s.ProjectSubjectResources)
            .FirstOrDefaultAsync(pks => pks.ProjectId == projectId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<ProjectModuleReferenceDto>> GetProjectModuleReferencesBySourceIdAsync(long sourceModuleId, CancellationToken cancellationToken = default)
    {
        var query = from pm in dbContext.Set<ProjectModule>()
                    join pks in dbContext.Set<ProjectKnowledgeStructure>() on pm.ProjectKnowledgeStructureId equals pks.Id
                    where pm.SourceModuleId == sourceModuleId
                    select new ProjectModuleReferenceDto(pks.ProjectId, pm.Id);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<ProjectQuestionReferenceDto>> GetProjectQuestionReferencesBySourceIdAsync(long sourceQuestionId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectQuestion>()
            .Where(pq => pq.SourceQuestionId == sourceQuestionId)
            .Select(pq => new ProjectQuestionReferenceDto(pq.ProjectBlock.ProjectId, pq.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetProjectsByIdsAsync(IEnumerable<long> projectIds, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .Where(p => projectIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetProjectsBySourceFormAsync(long sourceFormId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Project>()
            .Where(p => p.SourceFormId == sourceFormId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetProjectsBySourceModuleAsync(long sourceModuleId, CancellationToken cancellationToken = default)
    {
        // Use join query to avoid the internal property access issue
        var query = from project in dbContext.Set<Project>()
                    join knowledgeStructure in dbContext.Set<ProjectKnowledgeStructure>() on project.Id equals knowledgeStructure.ProjectId
                    join module in dbContext.Set<ProjectModule>() on knowledgeStructure.Id equals module.ProjectKnowledgeStructureId
                    where module.SourceModuleId == sourceModuleId
                    select project;

        return await query
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetProjectsByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Query from ProjectUsers table to find all projects associated with this user
        // Using raw SQL for now since ProjectUsers table is not yet mapped to EF
        var projectIds = await dbContext.Database
            .SqlQuery<long>($"SELECT ProjectId FROM [businessincubators].[ProjectUsers] WHERE UserId = {userId} AND IsActive = 1")
            .ToListAsync(cancellationToken);

        if (!projectIds.Any())
        {
            return [];
        }

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(p => projectIds.Contains(p.Id) && p.Status == ProjectStatus.Active)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return projects;
    }

    public async Task<List<ProjectSubjectReferenceDto>> GetProjectSubjectReferencesBySourceIdAsync(long sourceSubjectId, CancellationToken cancellationToken = default)
    {
        var query = from ps in dbContext.Set<ProjectSubject>()
                    join pt in dbContext.Set<ProjectTopic>() on ps.ProjectTopicId equals pt.Id
                    join pm in dbContext.Set<ProjectModule>() on pt.ProjectModuleId equals pm.Id
                    join pks in dbContext.Set<ProjectKnowledgeStructure>() on pm.ProjectKnowledgeStructureId equals pks.Id
                    where ps.SourceSubjectId == sourceSubjectId
                    select new ProjectSubjectReferenceDto(pks.ProjectId, ps.Id);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Project>> GetProjectsWithKnowledgeStructureAsync(long businessIncubatorId, Guid excludeProjectExternalId, CancellationToken cancellationToken = default)
    {
        // First get projects that have knowledge structures
        var projectIds = await dbContext.Set<ProjectKnowledgeStructure>()
            .Select(pks => pks.ProjectId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Then get the projects with their knowledge structures
        var projects = await dbContext.Set<Project>()
            .Where(p => p.BusinessIncubatorId == businessIncubatorId
                && !p.IsDeleted
                && p.ExternalId != excludeProjectExternalId
                && projectIds.Contains(p.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Load knowledge structures separately for the selected projects
        var projectIdsToLoad = projects.Select(p => p.Id).ToList();
        var knowledgeStructures = await dbContext.Set<ProjectKnowledgeStructure>()
            .Include("ProjectModules.ProjectTopics.ProjectSubjects")
            .Where(pks => projectIdsToLoad.Contains(pks.ProjectId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return projects;
    }

    public async Task<List<ProjectTopicReferenceDto>> GetProjectTopicReferencesBySourceIdAsync(long sourceTopicId, CancellationToken cancellationToken = default)
    {
        var query = from pt in dbContext.Set<ProjectTopic>()
                    join pm in dbContext.Set<ProjectModule>() on pt.ProjectModuleId equals pm.Id
                    join pks in dbContext.Set<ProjectKnowledgeStructure>() on pm.ProjectKnowledgeStructureId equals pks.Id
                    where pt.SourceTopicId == sourceTopicId
                    select new ProjectTopicReferenceDto(pks.ProjectId, pt.Id);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectWithBlocksByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .Include(p => p.ProjectBlocks)
                .ThenInclude(b => b.ProjectQuestions)
                    .ThenInclude(q => q.ProjectAnswerOptions)
            .FirstOrDefaultAsync(p => p.ExternalId == projectExternalId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Project?> GetProjectWithBlocksByIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Project>()
            .Include(p => p.ProjectBlocks)
                .ThenInclude(b => b.ProjectQuestions)
                    .ThenInclude(q => q.ProjectAnswerOptions)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectWithFormSubmissionsAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Project>()
            .Include(p => p.ProjectInvitations)
            .Include(p => p.FormSubmissions)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Project?> GetProjectWithInvitationsByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects
            .Include("ProjectInvitations")
            .Where(p => p.ExternalId == projectExternalId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return project;
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectWithKnowledgeStructureByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .Include("ProjectBlocks")
            .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectSubjects.ProjectSubjectResources")
            .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectSubjects.ProjectAnswerOptions")
            .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectQuestions.ProjectAnswerOptions")
            .FirstOrDefaultAsync(p => p.ExternalId == projectExternalId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Project?> GetProjectWithKnowledgeStructureByIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Project>()
            .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectSubjects.ProjectSubjectResources")
            .Include("ProjectBlocks.ProjectQuestions.ProjectAnswerOptions")
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Project?> GetProjectWithUsersAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .Include("ProjectUsers")
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Domain.DTOs.DashboardProjectData?> GetProjectDashboardDataAsync(
        long projectId,
        DateTime currentTime,
        DateTime? fromDate = null,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = fromDate ?? currentTime.AddDays(-30);
        var recentDate = currentTime.AddDays(-7);

        // First, check if project exists with basic info
        var projectInfo = await dbContext.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Key,
                p.BusinessIncubatorId,
                IncubatorName = dbContext.BusinessIncubators
                    .Where(b => b.Id == p.BusinessIncubatorId)
                    .Select(b => b.Name)
                    .FirstOrDefault() ?? string.Empty
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (projectInfo is null)
        {
            return null;
        }

        // Get user statistics using projection
        var userStats = await dbContext.Set<ProjectUser>()
            .AsNoTracking()
            .Where(pu => pu.ProjectId == projectId && pu.IsActive)
            .GroupBy(pu => 1)
            .Select(g => new
            {
                TotalUsers = g.Count(),
                RecentUsers = g.Count(u => u.JoinedAt > recentDate),
                UsersByRole = g.GroupBy(u => u.Role)
                    .Select(rg => new { Role = rg.Key, Count = rg.Count() })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalUsers = userStats?.TotalUsers ?? 0;
        var recentUsers = userStats?.RecentUsers ?? 0;
        var usersByRole = userStats?.UsersByRole?.ToDictionary(x => x.Role, x => x.Count) ?? new Dictionary<string, int>();

        // Get form statistics using projection
        var formStats = await dbContext.Set<ProjectFormSubmission>()
            .AsNoTracking()
            .Where(f => f.ProjectId == projectId)
            .GroupBy(f => 1)
            .Select(g => new
            {
                TotalForms = g.Count(),
                CompletedForms = g.Count(f =>
                    f.Status == ProjectFormSubmissionStatus.Submitted ||
                    f.Status == ProjectFormSubmissionStatus.Approved),
                InProgressForms = g.Count(f => f.Status == ProjectFormSubmissionStatus.Draft),
                PendingReviews = g.Count(f => f.Status == ProjectFormSubmissionStatus.Submitted),
                AvgCompletionHours = g
                    .Where(f => f.SubmittedAt.HasValue)
                    .Select(f => (double?)EF.Functions.DateDiffHour(f.StartedAt, f.SubmittedAt!.Value))
                    .Average() ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalForms = formStats?.TotalForms ?? 0;
        var completedForms = formStats?.CompletedForms ?? 0;
        var inProgressForms = formStats?.InProgressForms ?? 0;
        var totalPendingReviews = formStats?.PendingReviews ?? 0;
        var avgCompletionHours = formStats?.AvgCompletionHours ?? 0;

        // Get pending reviews (top 10)
        var pendingReviews = await dbContext.Set<ProjectFormSubmission>()
            .AsNoTracking()
            .Where(f => f.ProjectId == projectId && f.Status == ProjectFormSubmissionStatus.Submitted)
            .OrderByDescending(f => f.SubmittedAt)
            .Take(10)
            .Select(f => new Domain.DTOs.PendingReviewData
            {
                Id = f.Id,
                UserId = f.ParticipantUserId,
                SubmittedAt = f.SubmittedAt ?? f.StartedAt,
                DaysWaiting = EF.Functions.DateDiffDay(f.SubmittedAt ?? f.StartedAt, currentTime)
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Get recent form activities
        var formActivities = await dbContext.Set<ProjectFormSubmission>()
            .AsNoTracking()
            .Where(f => f.ProjectId == projectId && f.SubmittedAt > cutoffDate)
            .OrderByDescending(f => f.SubmittedAt)
            .Take(10)
            .Select(f => new Domain.DTOs.ActivityData
            {
                UserId = f.ParticipantUserId,
                Action = "form_submitted",
                Timestamp = f.SubmittedAt!.Value
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Get recent user activities
        var userActivities = await dbContext.Set<ProjectUser>()
            .AsNoTracking()
            .Where(u => u.ProjectId == projectId && u.IsActive && u.JoinedAt > cutoffDate)
            .OrderByDescending(u => u.JoinedAt)
            .Take(5)
            .Select(u => new Domain.DTOs.ActivityData
            {
                UserId = u.UserId,
                Action = "user_joined",
                Timestamp = u.JoinedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Combine and sort activities
        var recentActivities = formActivities
            .Concat(userActivities)
            .OrderByDescending(a => a.Timestamp)
            .Take(15)
            .ToList();

        // Get all unique user IDs for batch loading
        var allUserIds = await dbContext.Set<ProjectUser>()
            .AsNoTracking()
            .Where(pu => pu.ProjectId == projectId && pu.IsActive)
            .Select(pu => pu.UserId)
            .Union(
                dbContext.Set<ProjectFormSubmission>()
                    .Where(f => f.ProjectId == projectId)
                    .Select(f => f.ParticipantUserId))
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Get pending invitations count
        var pendingInvitations = await dbContext.Set<ProjectInvitation>()
            .AsNoTracking()
            .CountAsync(i => i.ProjectId == projectId && i.Status == ProjectInvitationStatus.Pending, cancellationToken)
            .ConfigureAwait(false);

        return new Domain.DTOs.DashboardProjectData
        {
            ProjectId = projectInfo.Id,
            ProjectName = projectInfo.Name,
            ProjectKey = projectInfo.Key,
            IncubatorId = projectInfo.BusinessIncubatorId,
            IncubatorName = projectInfo.IncubatorName,
            TotalUsers = totalUsers,
            ActiveUsers = totalUsers,
            RecentUsers = recentUsers,
            UsersByRole = usersByRole,
            TotalForms = totalForms,
            CompletedForms = completedForms,
            InProgressForms = inProgressForms,
            NotStartedForms = Math.Max(0, totalUsers - totalForms),
            AverageCompletionHours = avgCompletionHours,
            TotalPendingReviews = totalPendingReviews,
            PendingInvitations = pendingInvitations,
            PendingReviews = pendingReviews,
            RecentActivities = recentActivities,
            AllUserIds = allUserIds
        };
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectWithStagesByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .Include("_projectStages")
            .FirstOrDefaultAsync(p => p.ExternalId == projectExternalId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ProjectFormReview?> GetReviewByIdAsync(long reviewId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormReview>()
            .Include(r => r.FeedbackItems)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ProjectFormReview?> GetReviewBySubmissionIdAsync(long submissionId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormReview>()
            .Include(r => r.FeedbackItems)
            .Where(r => r.SubmissionId == submissionId)
            .OrderByDescending(r => r.ReviewedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ProjectFormReview>> GetReviewsBySubmissionIdAsync(long submissionId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormReview>()
            .Include(r => r.FeedbackItems)
            .Where(r => r.SubmissionId == submissionId)
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<long, Domain.ValueObjects.Module>> GetSourceModulesAsync(List<long> moduleIds, CancellationToken cancellationToken = default)
    {
        // This would typically fetch from a KnowledgeStructure context
        // For now, return an empty dictionary as the actual implementation
        // would depend on how source modules are stored
        return await Task.FromResult(new Dictionary<long, Domain.ValueObjects.Module>());
    }

    /// <inheritdoc/>
    public async Task<ProjectFormSubmission?> GetSubmissionByIdAsync(long submissionId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormSubmission>()
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ProjectFormSubmission?> GetSubmissionWithDetailsForReviewAsync(long submissionId, CancellationToken cancellationToken = default)
    {
        // Get submission with project details
        var submission = await dbContext.Set<ProjectFormSubmission>()
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken)
            .ConfigureAwait(false);

        if (submission is null)
        {
            return null;
        }

        // Load the project with its knowledge structure
        await dbContext.Entry(submission)
            .Reference("Project")
            .LoadAsync(cancellationToken)
            .ConfigureAwait(false);

        return submission;
    }

    public async Task<Domain.Aggregates.BusinessIncubator.BusinessIncubator?> GetWithProjectAndKnowledgeStructureByExternalId(Guid businessIncubatorExternalId, Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        var businessIncubator = await dbContext.BusinessIncubators
            .Where(bi => bi.ExternalId == businessIncubatorExternalId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (businessIncubator is null)
        {
            return businessIncubator;
        }

        // Load the project with knowledge structure separately
        var project = await dbContext.Projects
            .Where(p => p.BusinessIncubatorId == businessIncubator.Id && p.ExternalId == projectExternalId)
            .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectSubjects.ProjectSubjectResources")
            .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectSubjects.ProjectAnswerOptions")
            .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectQuestions.ProjectAnswerOptions")
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        // We can't assign the project to the business incubator due to DDD boundaries
        // The caller will need to handle the project separately
        return businessIncubator;
    }

    public async Task<Domain.Aggregates.BusinessIncubator.BusinessIncubator?> GetWithProjectBlocksByExternalId(Guid businessIncubatorExternalId, Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        var businessIncubator = await dbContext.BusinessIncubators
            .Where(bi => bi.ExternalId == businessIncubatorExternalId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (businessIncubator is null)
        {
            return businessIncubator;
        }

        // Load the projects for this business incubator
        var projects = await dbContext.Projects
            .Where(p => p.BusinessIncubatorId == businessIncubator.Id && p.ExternalId == projectExternalId)
            .Include("ProjectBlocks")
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return businessIncubator;
    }

    public Task<Domain.Aggregates.BusinessIncubator.BusinessIncubator?> GetWithProjectsByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default)
    {
        return dbContext.BusinessIncubators
            .Include("Projects")
            .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ProjectExistsWithKeyAsync(long businessIncubatorId, string projectKey, long? excludeProjectId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Projects
            .Where(p => p.BusinessIncubatorId == businessIncubatorId && p.Key == projectKey);

        if (excludeProjectId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProjectId.Value);
        }

        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ProjectExistsWithNameAsync(long businessIncubatorId, string projectName, long? excludeProjectId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Projects
            .Where(p => p.BusinessIncubatorId == businessIncubatorId && p.Name == projectName);

        if (excludeProjectId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProjectId.Value);
        }

        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ProjectHasBlockWithNameAsync(long projectId, string blockName, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProjectBlocks
            .AnyAsync(pb => pb.ProjectId == projectId && pb.Name == blockName, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ProjectHasBlockWithSourceIdAsync(long projectId, long sourceBlockId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProjectBlocks
            .AnyAsync(pb => pb.ProjectId == projectId && pb.SourceBlockId == sourceBlockId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Update(Project project)
    {
        dbContext.Projects.Update(project);
    }

    /// <inheritdoc/>
    public void Update(ProjectInvitation invitation)
    {
        dbContext.ProjectInvitations.Update(invitation);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        Update(project);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task UpdateProjectsAsync(List<Project> projects, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Project>().UpdateRange(projects);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task UpdateReviewAsync(ProjectFormReview review, CancellationToken cancellationToken = default)
    {
        dbContext.Set<ProjectFormReview>().Update(review);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateSubmissionAsync(ProjectFormSubmission submission, CancellationToken cancellationToken = default)
    {
        dbContext.Set<ProjectFormSubmission>().Update(submission);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<long, ProjectQuestionValidationDto>> ValidateProjectQuestionsAsync(
        Guid projectExternalId,
        List<long> questionIds,
        CancellationToken cancellationToken = default)
    {
        // Get questions with their answer options
        var query = from project in dbContext.Projects
                    join knowledgeStructure in dbContext.ProjectKnowledgeStructures on project.Id equals knowledgeStructure.ProjectId
                    join module in dbContext.ProjectModules on knowledgeStructure.Id equals module.ProjectKnowledgeStructureId
                    join topic in dbContext.ProjectTopics on module.Id equals topic.ProjectModuleId
                    join question in dbContext.ProjectQuestions on topic.Id equals question.ProjectTopicId
                    where project.ExternalId == projectExternalId && questionIds.Contains(question.Id)
                    select new
                    {
                        question.Id,
                        AnswerType = (int)question.AnswerType,
                    };

        var questions = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        if (!questions.Any())
        {
            return [];
        }

        // Get answer options for the questions
        var foundQuestionIds = questions.Select(q => q.Id).ToList();
        var answerOptions = await dbContext.ProjectAnswerOptions
            .Where(ao => foundQuestionIds.Contains(ao.ProjectQuestionId))
            .Select(ao => new { ao.ProjectQuestionId, ao.Id })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Build the validation dictionary
        var result = questions.ToDictionary(
            q => q.Id,
            q => new ProjectQuestionValidationDto(
                q.Id,
                q.AnswerType,
                answerOptions.Where(ao => ao.ProjectQuestionId == q.Id).Select(ao => ao.Id).ToList()));

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProjectFormSubmission?> GetFormSubmissionAsync(
        long projectId,
        string userId,
        QuestionPhase phase,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormSubmission>()
            .FirstOrDefaultAsync(s =>
                s.ProjectId == projectId &&
                s.ParticipantUserId == userId &&
                s.Phase == phase,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ProjectFormSubmission?> GetFormSubmissionByIdAsync(
        long submissionId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormSubmission>()
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ProjectFormSubmission?> GetFormSubmissionByExternalIdAsync(
        Guid externalId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormSubmission>()
            .FirstOrDefaultAsync(s => s.ExternalId == externalId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ProjectFormSubmission?> GetFormSubmissionWithDetailsByExternalIdAsync(
        Guid externalId,
        CancellationToken cancellationToken = default)
    {
        // For now, just get the submission without additional includes
        // Can be enhanced later with proper navigation properties
        return await dbContext.Set<ProjectFormSubmission>()
            .FirstOrDefaultAsync(s => s.ExternalId == externalId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void AddFormSubmission(ProjectFormSubmission submission)
    {
        dbContext.Set<ProjectFormSubmission>().Add(submission);
    }

    /// <inheritdoc/>
    public Task UpdateFormSubmissionAsync(
        ProjectFormSubmission submission,
        CancellationToken cancellationToken = default)
    {
        dbContext.Set<ProjectFormSubmission>().Update(submission);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectWithStagesAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Project>()
            .Include("_projectStages")
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> IsUserProjectParticipantAsync(long projectId, string userId, CancellationToken cancellationToken = default)
    {
        // Check ProjectUsers - this is the primary way to track participants
        var projectUserExists = await dbContext.Set<ProjectUser>()
            .AnyAsync(pu =>
                pu.ProjectId == projectId &&
                pu.UserId == userId &&
                pu.IsActive,
                cancellationToken)
            .ConfigureAwait(false);

        return projectUserExists;
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetProjectsByIncubatorIdAsync(long incubatorId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Project>()
            .Where(p => p.BusinessIncubatorId == incubatorId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ProjectFormFeedback?> GetFeedbackByIdAsync(long feedbackId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormFeedback>()
            .Include(f => f.ParentFeedback)
            .Include(f => f.Replies)
            .FirstOrDefaultAsync(f => f.Id == feedbackId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ProjectFormFeedback>> GetFeedbackWithRepliesForSubmissionAsync(long submissionId, CancellationToken cancellationToken = default)
    {
        // Get the review for this submission
        var reviews = await dbContext.Set<ProjectFormReview>()
            .Where(r => r.SubmissionId == submissionId)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!reviews.Any())
        {
            return new List<ProjectFormFeedback>();
        }

        // Get all feedback (parent and replies) for these reviews
        return await dbContext.Set<ProjectFormFeedback>()
            .Where(f => reviews.Contains(f.ReviewId) && !f.IsDeleted)
            .OrderBy(f => f.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddFeedbackAsync(ProjectFormFeedback feedback, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<ProjectFormFeedback>().AddAsync(feedback, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void UpdateFeedback(ProjectFormFeedback feedback)
    {
        dbContext.Set<ProjectFormFeedback>().Update(feedback);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<long, ProjectQuestion>> GetProjectQuestionsWithAnswerOptionsAsync(
        long projectId,
        QuestionPhase phase,
        CancellationToken cancellationToken = default)
    {
        // Get all questions for the project that apply to the specified phase
        // Using a join approach since navigation properties are internal
        var questions = await (from q in dbContext.Set<ProjectQuestion>()
                               join t in dbContext.Set<ProjectTopic>() on q.ProjectTopicId equals t.Id
                               join m in dbContext.Set<ProjectModule>() on t.ProjectModuleId equals m.Id
                               join k in dbContext.Set<ProjectKnowledgeStructure>() on m.ProjectKnowledgeStructureId equals k.Id
                               where k.ProjectId == projectId && q.AppliesToPhase == phase
                               select q)
            .Include(q => q.ProjectAnswerOptions)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return questions.ToDictionary(q => q.Id);
    }

    /// <inheritdoc/>
    public async Task<List<ProjectAnswerOption>> GetAnswerOptionsByIdsAsync(
        List<long> answerOptionIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectAnswerOption>()
            .Where(ao => answerOptionIds.Contains(ao.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ProjectFormSubmission>> GetAllSubmissionsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormSubmission>()
            .OrderByDescending(s => s.SubmittedAt ?? s.StartedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ProjectFormSubmission>> GetSubmissionsByProjectIdsAsync(long[] projectIds, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProjectFormSubmission>()
            .Where(s => projectIds.Contains(s.ProjectId))
            .OrderByDescending(s => s.SubmittedAt ?? s.StartedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetProjectsInGeohashesAsync(
        HashSet<string> geohashPrefixes,
        decimal minLat,
        decimal maxLat,
        decimal minLon,
        decimal maxLon,
        CancellationToken cancellationToken = default)
    {
        // For simplicity and performance, we'll rely on the bounding box filter
        // The geohash prefixes were mainly for optimization, but the bounding box
        // is sufficient for our needs and works well with SQL Server indexes
        return await dbContext.Set<Project>()
            .Include("BusinessIncubator")
            .Where(p => !p.IsDeleted)
            .Where(p => p.Status == ProjectStatus.Active)
            .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
            .Where(p => p.Latitude >= minLat && p.Latitude <= maxLat)
            .Where(p => p.Longitude >= minLon && p.Longitude <= maxLon)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<object?> GetProjectInterestAsync(
        long projectId,
        string userId,
        string interestType,
        CancellationToken cancellationToken = default)
    {
        // Direct SQL query since ProjectInterests is not a mapped entity
        var sql = @"
            SELECT TOP 1 * 
            FROM [businessincubators].[ProjectInterests]
            WHERE [ProjectId] = @projectId 
            AND [ObserverUserId] = @userId 
            AND [InterestType] = @interestType";

        var connection = dbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@projectId", projectId));
        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@userId", userId));
        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@interestType", interestType));

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new { Id = reader.GetInt64(0) }; // Return a simple object with ID
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task RecordProjectInterestAsync(
        long projectId,
        string? userId,
        string? sessionId,
        string interestType,
        decimal? observerLatitude,
        decimal? observerLongitude,
        double? distance,
        string? userAgent,
        string? ipAddress,
        string? referrerUrl,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            INSERT INTO [businessincubators].[ProjectInterests]
            ([ProjectId], [ObserverUserId], [ObserverSessionId], [InterestType], 
             [ObserverLatitude], [ObserverLongitude], [Distance],
             [UserAgent], [IpAddress], [ReferrerUrl], [CreatedAt])
            VALUES
            (@projectId, @userId, @sessionId, @interestType,
             @latitude, @longitude, @distance,
             @userAgent, @ipAddress, @referrerUrl, @createdAt)";

        await dbContext.Database.ExecuteSqlRawAsync(
            sql,
            new Microsoft.Data.SqlClient.SqlParameter("@projectId", projectId),
            new Microsoft.Data.SqlClient.SqlParameter("@userId", (object?)userId ?? DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@sessionId", (object?)sessionId ?? DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@interestType", interestType),
            new Microsoft.Data.SqlClient.SqlParameter("@latitude", (object?)observerLatitude ?? DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@longitude", (object?)observerLongitude ?? DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@distance", (object?)distance ?? DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@userAgent", (object?)userAgent ?? DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@ipAddress", (object?)ipAddress ?? DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@referrerUrl", (object?)referrerUrl ?? DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@createdAt", createdAt));
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetActiveProjectsWithStagesAsync(CancellationToken cancellationToken = default)
    {
        // Load projects with their stages and users
        // Use string-based Include with proper navigation names
        var projects = await dbContext.Projects
            .Include("_projectStages") // Private field for ProjectStages (public property is ignored in EF)
            .Include("ProjectUsers") // Navigation configured with backing field
            .Where(p => !p.IsDeleted && p.Status == ProjectStatus.Active)
            .ToListAsync(cancellationToken);

        return projects;
    }
}
