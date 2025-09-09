using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectFormStructure;

/// <summary>
/// Handler for getting the project form structure.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetProjectFormStructureQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public sealed class GetProjectFormStructureQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetProjectFormStructureQuery, ProjectFormStructureDto>
{

    /// <inheritdoc/>
    public override async Task<Result<ProjectFormStructureDto>> Handle(GetProjectFormStructureQuery request, CancellationToken cancellationToken)
    {
        // Get the project knowledge structure with full details
        var knowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(request.ProjectId, cancellationToken).ConfigureAwait(false);

        if (knowledgeStructure is null)
        {
            return Failure(ResultErrorCodes.KnowledgeStructure_NotFound, ("KnowledgeStructure", "El proyecto no tiene una estructura de conocimiento configurada."));
        }

        // Also get the project with blocks to have the ProjectBlock entities
        var projectWithBlocks = await repository.GetProjectWithBlocksByIdAsync(request.ProjectId, cancellationToken).ConfigureAwait(false);

        if (projectWithBlocks is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, ("Project", "Proyecto no encontrado."));
        }

        // Get all blocks and questions organized by blocks
        var blocks = new List<ProjectFormBlockDto>();
        var blockDict = new Dictionary<long, ProjectFormBlockDto>();

        // First, collect all blocks from the knowledge structure
        foreach (var module in knowledgeStructure.ProjectModules.OrderBy(m => m.Order))
        {
            foreach (var topic in module.ProjectTopics.OrderBy(t => t.Order))
            {
                // Group questions by block
                var questionsByBlock = topic.ProjectQuestions
                    .Where(q => q.IsUsedForDiagnosis) // Only include questions used for diagnosis
                    .GroupBy(q => q.ProjectBlockId)
                    .OrderBy(g => g.Min(q => q.Order));

                foreach (var blockGroup in questionsByBlock)
                {
                    var blockId = blockGroup.Key;
                    if (!blockDict.TryGetValue(blockId, out var blockDto))
                    {
                        // Find the block from the project blocks
                        var projectBlock = projectWithBlocks.ProjectBlocks.FirstOrDefault(b => b.Id == blockId);
                        blockDto = new ProjectFormBlockDto
                        {
                            BlockId = blockId,
                            BlockName = projectBlock?.Name ?? $"Bloque {blockId}",
                            Order = blocks.Count + 1,
                            Questions = []
                        };
                        blockDict[blockId] = blockDto;
                        blocks.Add(blockDto);
                    }

                    // Add questions to the block
                    foreach (var question in blockGroup.OrderBy(q => q.Order))
                    {
                        blockDto.Questions.Add(new ProjectFormQuestionDto
                        {
                            QuestionId = question.Id,
                            QuestionText = question.Text,
                            IsRequired = true, // All questions are required by default
                            AnswerType = (int)question.AnswerType,
                            AnswerTypeName = GetAnswerTypeName(question.AnswerType),
                            Order = question.Order,
                            TopicId = topic.Id,
                            IsUsedForDiagnosis = question.IsUsedForDiagnosis,
                            IsUsedForMentoringPlan = question.IsUsedForMentoringPlan,
                            AppliesToPhase = (int)question.AppliesToPhase,
                            ModuleInfo = new ProjectModuleInfoDto
                            {
                                ModuleId = module.Id,
                                ModuleName = module.Name
                            },
                            TopicInfo = new ProjectTopicInfoDto
                            {
                                TopicId = topic.Id,
                                TopicName = topic.Name
                            },
                            AnswerOptions = question.ProjectAnswerOptions
                                .OrderBy(ao => ao.Order)
                                .Select(ao => new ProjectFormAnswerOptionDto
                                {
                                    AnswerOptionId = ao.Id,
                                    OptionText = ao.Text,
                                    Score = ao.Score,
                                    Order = ao.Order,
                                    RequiresFollowUp = !string.IsNullOrEmpty(ao.FollowUpQuestionText),
                                    FollowUpQuestionText = ao.FollowUpQuestionText
                                })
                                .ToList()
                        });
                    }
                }
            }
        }

        var result = new ProjectFormStructureDto
        {
            FormName = knowledgeStructure.Name,
            Description = knowledgeStructure.Description,
            ProjectKnowledgeStructureId = knowledgeStructure.Id,
            Blocks = blocks
        };

        return Success(result);
    }

    private static string GetAnswerTypeName(AnswerType answerType)
    {
        return answerType switch
        {
            AnswerType.SingleChoice => "Selección única",
            AnswerType.MultiChoice => "Selección múltiple",
            AnswerType.FreeText => "Texto libre",
            AnswerType.Numeric => "Numérico",
            AnswerType.Date => "Fecha",
            AnswerType.PersonId => "Identificación",
            AnswerType.IdType => "Tipo de identificación",
            AnswerType.Gender => "Género",
            AnswerType.MaritalStatus => "Estado civil",
            AnswerType.Email => "Correo electrónico",
            AnswerType.PhoneNumber => "Teléfono",
            AnswerType.Nationality => "Nacionalidad",
            _ => answerType.ToString()
        };
    }
}