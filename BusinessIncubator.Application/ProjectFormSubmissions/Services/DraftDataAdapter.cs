using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;
using LinaSys.BusinessIncubator.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Services;

/// <summary>
/// Default implementation of IDraftDataAdapter.
/// Adapts draft data from older form versions to match the current form structure.
/// </summary>
public class DraftDataAdapter(ILogger<DraftDataAdapter> logger) : IDraftDataAdapter
{

    /// <inheritdoc/>
    public async Task<DraftDataDto> AdaptToCurrentVersionAsync(
        DraftDataDto draft,
        int draftVersion,
        int currentVersion,
        Domain.Aggregates.BusinessIncubator.ProjectKnowledgeStructure structure)
    {
        if (!IsAdaptationNeeded(draftVersion, currentVersion))
        {
            return draft;
        }

        logger.LogInformation(
            "Adapting draft from version {DraftVersion} to {CurrentVersion}",
            draftVersion,
            currentVersion);

        // Create a new draft data structure based on the current form
        var adaptedDraft = new DraftDataDto
        {
            FormVersion = currentVersion,
            LastSavedAt = draft.LastSavedAt,
            CurrentBlockIndex = draft.CurrentBlockIndex,
            ProgressPercentage = 0, // Will be recalculated
            BlockResponses = []
        };

        // Get all current questions from the knowledge structure
        var currentQuestions = GetAllQuestions(structure);
        var draftQuestionMap = CreateQuestionResponseMap(draft);

        // Group questions by block (for now we'll create a single block)
        // In a real implementation, you'd group by actual blocks from the project
        var blockResponse = new BlockResponseDto
        {
            BlockId = 1, // Default block ID
            BlockName = "Formulario Principal",
            IsCompleted = false,
            QuestionResponses = []
        };

        foreach (var question in currentQuestions)
        {
            var response = AdaptQuestionResponse(question, draftQuestionMap);
            blockResponse.QuestionResponses.Add(response);
        }

        // Check if block is completed
        blockResponse.IsCompleted = blockResponse.QuestionResponses.All(q => q.IsAnswered);
        adaptedDraft.BlockResponses.Add(blockResponse);

        // Recalculate progress
        var totalQuestions = blockResponse.QuestionResponses.Count;
        var answeredQuestions = blockResponse.QuestionResponses.Count(q => q.IsAnswered);
        adaptedDraft.ProgressPercentage = totalQuestions > 0
            ? (decimal)answeredQuestions / totalQuestions * 100
            : 0;

        await Task.CompletedTask; // Async for future enhancements
        return adaptedDraft;
    }

    /// <inheritdoc/>
    public bool IsAdaptationNeeded(int draftVersion, int currentVersion)
    {
        return draftVersion < currentVersion;
    }

    /// <inheritdoc/>
    public async Task<AdaptationSummary> GetAdaptationSummaryAsync(
        DraftDataDto draft,
        int draftVersion,
        int currentVersion,
        Domain.Aggregates.BusinessIncubator.ProjectKnowledgeStructure structure)
    {
        var summary = new AdaptationSummary();

        if (!IsAdaptationNeeded(draftVersion, currentVersion))
        {
            return summary;
        }

        var currentQuestions = GetAllQuestions(structure);
        var draftQuestionMap = CreateQuestionResponseMap(draft);

        // Find added questions (in current structure but not in draft)
        foreach (var question in currentQuestions)
        {
            if (!draftQuestionMap.ContainsKey(question.Id))
            {
                summary.AddedQuestions.Add(new QuestionChange
                {
                    QuestionId = question.Id,
                    QuestionText = question.Text,
                    ChangeReason = "Nueva pregunta añadida en la versión actual",
                    NewAnswerType = question.AnswerType.ToString()
                });
            }
        }

        // Find removed questions (in draft but not in current structure)
        var currentQuestionIds = currentQuestions.Select(q => q.Id).ToHashSet();
        foreach (var kvp in draftQuestionMap)
        {
            if (!currentQuestionIds.Contains(kvp.Key))
            {
                summary.RemovedQuestions.Add(new QuestionChange
                {
                    QuestionId = kvp.Key,
                    QuestionText = kvp.Value.QuestionText,
                    ChangeReason = "Pregunta eliminada en la versión actual"
                });
            }
        }

        // Check for type changes in existing questions
        foreach (var question in currentQuestions)
        {
            if (draftQuestionMap.TryGetValue(question.Id, out var draftResponse))
            {
                // Check if answer type changed
                if (draftResponse.AnswerType != (int)question.AnswerType)
                {
                    summary.ModifiedQuestions.Add(new QuestionChange
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        ChangeReason = "Tipo de respuesta cambiado",
                        RequiresConversion = true,
                        OldAnswerType = ((AnswerType)draftResponse.AnswerType).ToString(),
                        NewAnswerType = question.AnswerType.ToString()
                    });
                }
            }
        }

        await Task.CompletedTask; // Async for future enhancements
        return summary;
    }

    private QuestionResponseDto AdaptQuestionResponse(
        Domain.Aggregates.BusinessIncubator.ProjectQuestion question,
        Dictionary<long, QuestionResponseDto> draftQuestionMap)
    {
        // Try to find existing answer in draft
        if (draftQuestionMap.TryGetValue(question.Id, out var existingResponse))
        {
            // Check if answer type matches
            if (existingResponse.AnswerType == (int)question.AnswerType)
            {
                // Return existing response with updated question text
                existingResponse.QuestionText = question.Text;
                return existingResponse;
            }

            // Answer type changed, try to convert or provide default
            return new QuestionResponseDto
            {
                QuestionId = question.Id,
                QuestionText = question.Text,
                AnswerType = (int)question.AnswerType,
                IsAnswered = false,
                // Preserve module/topic info if available
                ModuleInfo = existingResponse.ModuleInfo,
                TopicInfo = existingResponse.TopicInfo
            };
        }

        // Question doesn't exist in draft, provide default
        return CreateDefaultQuestionResponse(question);
    }

    private QuestionResponseDto CreateDefaultQuestionResponse(Domain.Aggregates.BusinessIncubator.ProjectQuestion question)
    {
        var response = new QuestionResponseDto
        {
            QuestionId = question.Id,
            QuestionText = question.Text,
            AnswerType = (int)question.AnswerType,
            IsAnswered = false
        };

        // Note: Module and topic info would need to be passed from the caller
        // since questions don't have direct navigation to their parent topic/module
        return response;
    }

    private List<Domain.Aggregates.BusinessIncubator.ProjectQuestion> GetAllQuestions(
        Domain.Aggregates.BusinessIncubator.ProjectKnowledgeStructure structure)
    {
        var questions = new List<Domain.Aggregates.BusinessIncubator.ProjectQuestion>();

        // Get questions from modules -> topics
        foreach (var module in structure.ProjectModules)
        {
            foreach (var topic in module.ProjectTopics)
            {
                questions.AddRange(topic.ProjectQuestions);
            }
        }

        return questions;
    }

    private Dictionary<long, QuestionResponseDto> CreateQuestionResponseMap(DraftDataDto draft)
    {
        var map = new Dictionary<long, QuestionResponseDto>();

        foreach (var block in draft.BlockResponses ?? Enumerable.Empty<BlockResponseDto>())
        {
            foreach (var response in block.QuestionResponses ?? Enumerable.Empty<QuestionResponseDto>())
            {
                map[response.QuestionId] = response;
            }
        }

        return map;
    }
}
