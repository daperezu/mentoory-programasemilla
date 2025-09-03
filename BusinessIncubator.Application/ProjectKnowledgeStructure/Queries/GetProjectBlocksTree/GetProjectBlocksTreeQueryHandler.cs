using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructureTree;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectBlocksTree;

/// <summary>
/// Handler for getting the project blocks as a tree.
/// </summary>
public sealed class GetProjectBlocksTreeQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetProjectBlocksTreeQueryHandler> logger)
    : BaseCommandHandler<GetProjectBlocksTreeQuery, List<TreeNodeDto>>
{
    public override async Task<Result<List<TreeNodeDto>>> Handle(
        GetProjectBlocksTreeQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project with blocks
            var project = await repository.GetProjectWithBlocksByExternalIdAsync(request.ProjectExternalId, cancellationToken);

            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Proyecto no encontrado"));
            }

            // Build the tree structure based on blocks
            var treeNodes = new List<TreeNodeDto>();

            // Add project as root node
            var rootNode = new TreeNodeDto
            {
                Id = $"project_{project.Id}",
                Text = project.Name,
                Type = "root",
                Icon = "fas fa-project-diagram",
                State = new TreeNodeStateDto { Opened = true }
            };

            // Add blocks
            if (project.ProjectBlocks.Any())
            {
                foreach (var block in project.ProjectBlocks.OrderBy(b => b.SourceBlockId ?? b.Id))
                {
                    var blockNode = new TreeNodeDto
                    {
                        Id = $"block_{block.Id}",
                        Text = block.Name,
                        Type = "block",
                        Icon = "fas fa-layer-group",
                        State = new TreeNodeStateDto { Opened = false },
                        Data = new TreeNodeDataDto
                        {
                            EntityId = block.Id,
                            SourceId = block.SourceBlockId,
                            IsCustomized = block.IsNameCustomized
                        }
                    };

                    // Add questions under each block
                    if (block.ProjectQuestions.Any())
                    {
                        foreach (var question in block.ProjectQuestions.OrderBy(q => q.Order))
                        {
                            var questionNode = new TreeNodeDto
                            {
                                Id = $"question_{question.Id}",
                                Text = question.Text.Length > 100
                                    ? question.Text.Substring(0, 97) + "..."
                                    : question.Text,
                                Type = "question",
                                Icon = "fas fa-question-circle",
                                State = new TreeNodeStateDto { Opened = false },
                                Data = new TreeNodeDataDto
                                {
                                    EntityId = question.Id,
                                    SourceId = question.SourceQuestionId,
                                    IsCustomized = question.IsTextCustomized || question.IsAnswerTypeCustomized,
                                    CustomizationStatus = GetCustomizationStatus(
                                        question.SourceQuestionId,
                                        question.IsTextCustomized || question.IsAnswerTypeCustomized),
                                    // Additional data for questions
                                    AnswerType = question.AnswerType.ToString(),
                                    IsUsedForDiagnosis = question.IsUsedForDiagnosis,
                                    AppliesToPhase = question.AppliesToPhase.ToString()
                                }
                            };

                            // If the question has a topic, add it as metadata
                            if (question.ProjectTopicId.HasValue)
                            {
                                questionNode.Data.TopicId = question.ProjectTopicId.Value;
                                questionNode.Data.TopicInfo = "(Asociada a tema)";
                            }

                            // Add answer options under questions
                            if (question.ProjectAnswerOptions.Any())
                            {
                                foreach (var answerOption in question.ProjectAnswerOptions.OrderBy(ao => ao.Order))
                                {
                                    var answerNode = new TreeNodeDto
                                    {
                                        Id = $"answer_{answerOption.Id}",
                                        Text = answerOption.Text,
                                        Type = "answer",
                                        Icon = "fas fa-check-circle",
                                        State = new TreeNodeStateDto { Opened = false },
                                        Data = new TreeNodeDataDto
                                        {
                                            EntityId = answerOption.Id,
                                            SourceId = answerOption.SourceAnswerOptionId,
                                            IsCustomized = answerOption.IsTextCustomized,
                                            Score = answerOption.Score,
                                            Foda = answerOption.Foda.ToString(),
                                            Odsr = answerOption.Odsr.ToString(),
                                            FollowUpQuestionText = answerOption.FollowUpQuestionText,
                                            FodaExplanation = answerOption.FodaExplanation,
                                            OdsrExplanation = answerOption.OdsrExplanation
                                        }
                                    };
                                    questionNode.Children.Add(answerNode);
                                }
                            }

                            blockNode.Children.Add(questionNode);
                        }
                    }
                    else
                    {
                        // Block has no questions - add a placeholder
                        var emptyNode = new TreeNodeDto
                        {
                            Id = $"empty_block_{block.Id}",
                            Text = "(Sin preguntas)",
                            Type = "empty",
                            Icon = "fas fa-info-circle text-muted",
                            State = new TreeNodeStateDto { Opened = false }
                        };
                        blockNode.Children.Add(emptyNode);
                    }

                    rootNode.Children.Add(blockNode);
                }
            }
            else
            {
                // No blocks - add a placeholder message
                var emptyNode = new TreeNodeDto
                {
                    Id = "empty_blocks",
                    Text = "(Sin bloques)",
                    Type = "empty",
                    Icon = "fas fa-info-circle text-muted",
                    State = new TreeNodeStateDto { Opened = false }
                };
                rootNode.Children.Add(emptyNode);
            }

            treeNodes.Add(rootNode);
            return Success(treeNodes);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error getting project blocks tree for project {ProjectExternalId}",
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown, ("BlocksTree", "Error al obtener los bloques del proyecto"));
        }
    }

    private static string GetCustomizationStatus(long? sourceId, bool isCustomized)
    {
        if (!sourceId.HasValue)
        {
            return "custom";
        }

        return isCustomized ? "customized" : "synced";
    }
}