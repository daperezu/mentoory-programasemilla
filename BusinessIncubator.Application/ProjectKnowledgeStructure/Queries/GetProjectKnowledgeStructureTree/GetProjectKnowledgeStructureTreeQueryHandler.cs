using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application;
namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructureTree;

/// <summary>
/// Handler for getting the project knowledge structure as a tree.
/// </summary>
public sealed class GetProjectKnowledgeStructureTreeQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetProjectKnowledgeStructureTreeQueryHandler> logger)
    : BaseCommandHandler<GetProjectKnowledgeStructureTreeQuery, List<TreeNodeDto>>
{
    public override async Task<Result<List<TreeNodeDto>>> Handle(
        GetProjectKnowledgeStructureTreeQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project directly
            var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);

            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Proyecto no encontrado"));
            }

            // Get the project knowledge structure
            var knowledgeStructure = await repository
                .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

            if (knowledgeStructure is null)
            {
                return Success([]);
            }

            // Build the tree structure
            var treeNodes = new List<TreeNodeDto>();

            // Add knowledge structure as root node
            var rootNode = new TreeNodeDto
            {
                Id = $"structure_{knowledgeStructure.Id}",
                Text = knowledgeStructure.Name,
                Type = "structure",
                Icon = "fas fa-sitemap",
                State = new TreeNodeStateDto { Opened = true },
                Data = new TreeNodeDataDto
                {
                    EntityId = knowledgeStructure.Id,
                    SourceId = knowledgeStructure.SourceKnowledgeStructureId,
                    IsCustomized = knowledgeStructure.IsNameCustomized || knowledgeStructure.IsDescriptionCustomized,
                    CustomizationStatus = GetCustomizationStatus(
                        knowledgeStructure.SourceKnowledgeStructureId,
                        knowledgeStructure.IsNameCustomized || knowledgeStructure.IsDescriptionCustomized)
                }
            };

            // Add modules if any exist
            if (knowledgeStructure.ProjectModules.Any())
            {
                foreach (var module in knowledgeStructure.ProjectModules.OrderBy(m => m.Order))
                {
                    var moduleNode = new TreeNodeDto
                    {
                        Id = $"module_{module.Id}",
                        Text = module.Name,
                        Type = "module",
                        Icon = "fas fa-folder",
                        State = new TreeNodeStateDto { Opened = false },
                        Data = new TreeNodeDataDto
                        {
                            EntityId = module.Id,
                            SourceId = module.SourceModuleId,
                            IsCustomized = module.IsNameCustomized || module.IsOrderCustomized,
                            CustomizationStatus = GetCustomizationStatus(
                                module.SourceModuleId,
                                module.IsNameCustomized || module.IsOrderCustomized)
                        }
                    };

                    // Add topics if any exist
                    if (module.ProjectTopics.Any())
                    {
                        foreach (var topic in module.ProjectTopics.OrderBy(t => t.Order))
                        {
                            var topicNode = new TreeNodeDto
                            {
                                Id = $"topic_{topic.Id}",
                                Text = topic.Name,
                                Type = "topic",
                                Icon = "fas fa-book",
                                State = new TreeNodeStateDto { Opened = false },
                                Data = new TreeNodeDataDto
                                {
                                    EntityId = topic.Id,
                                    SourceId = topic.SourceTopicId,
                                    IsCustomized = topic.IsNameCustomized || topic.IsOrderCustomized,
                                    CustomizationStatus = GetCustomizationStatus(
                                        topic.SourceTopicId,
                                        topic.IsNameCustomized || topic.IsOrderCustomized)
                                }
                            };

                            // Add questions linked to this topic
                            if (topic.ProjectQuestions.Any())
                            {
                                foreach (var question in topic.ProjectQuestions.OrderBy(q => q.Order))
                                {
                                    var questionNode = new TreeNodeDto
                                    {
                                        Id = $"question_{question.Id}",
                                        Text = question.Text,
                                        Type = "question",
                                        Icon = "fas fa-question-circle",
                                        State = new TreeNodeStateDto { Opened = false },
                                        Data = new TreeNodeDataDto
                                        {
                                            EntityId = question.Id,
                                            SourceId = question.SourceQuestionId,
                                            IsCustomized = question.IsTextCustomized ||
                                                          question.IsAnswerTypeCustomized ||
                                                          question.IsAppliesToPhaseCustomized ||
                                                          question.IsDiagnosisCustomized,
                                            CustomizationStatus = GetCustomizationStatus(
                                                question.SourceQuestionId,
                                                question.IsTextCustomized ||
                                                question.IsAnswerTypeCustomized ||
                                                question.IsAppliesToPhaseCustomized ||
                                                question.IsDiagnosisCustomized),
                                            Order = question.Order,
                                            AnswerType = question.AnswerType.ToString(),
                                            IsUsedForDiagnosis = question.IsUsedForDiagnosis,
                                            AppliesToPhase = question.AppliesToPhase.ToString(),
                                            ProjectBlockId = question.ProjectBlockId
                                        }
                                    };

                                    // Add answer options if any
                                    if (question.ProjectAnswerOptions.Any())
                                    {
                                        foreach (var answer in question.ProjectAnswerOptions.OrderBy(a => a.Order))
                                        {
                                            var answerNode = new TreeNodeDto
                                            {
                                                Id = $"answer_{answer.Id}",
                                                Text = $"{answer.Text} (Puntaje: {answer.Score})",
                                                Type = "answer",
                                                Icon = "fas fa-check-circle",
                                                State = new TreeNodeStateDto { Opened = false },
                                                Data = new TreeNodeDataDto
                                                {
                                                    EntityId = answer.Id,
                                                    SourceId = answer.SourceAnswerOptionId,
                                                    IsCustomized = answer.IsTextCustomized ||
                                                                  answer.IsScoreCustomized ||
                                                                  answer.IsFodaCustomized ||
                                                                  answer.IsOdsrCustomized,
                                                    CustomizationStatus = GetCustomizationStatus(
                                                        answer.SourceAnswerOptionId,
                                                        answer.IsTextCustomized ||
                                                        answer.IsScoreCustomized ||
                                                        answer.IsFodaCustomized ||
                                                        answer.IsOdsrCustomized),
                                                    Score = answer.Score,
                                                    Foda = answer.Foda.ToString(),
                                                    Odsr = answer.Odsr.ToString(),
                                                    FollowUpQuestionText = answer.FollowUpQuestionText,
                                                    FodaExplanation = answer.FodaExplanation,
                                                    OdsrExplanation = answer.OdsrExplanation
                                                }
                                            };

                                            questionNode.Children.Add(answerNode);
                                        }
                                    }

                                    topicNode.Children.Add(questionNode);
                                }
                            }

                            // Add subjects
                            foreach (var subject in topic.ProjectSubjects.OrderBy(s => s.Order))
                            {
                                var subjectNode = new TreeNodeDto
                                {
                                    Id = $"subject_{subject.Id}",
                                    Text = subject.Title,
                                    Type = "subject",
                                    Icon = "fas fa-file-alt",
                                    State = new TreeNodeStateDto { Opened = false },
                                    Data = new TreeNodeDataDto
                                    {
                                        EntityId = subject.Id,
                                        SourceId = subject.SourceSubjectId,
                                        IsCustomized = subject.IsTitleCustomized || subject.IsContentCustomized || subject.IsOrderCustomized,
                                        CustomizationStatus = GetCustomizationStatus(
                                            subject.SourceSubjectId,
                                            subject.IsTitleCustomized || subject.IsContentCustomized || subject.IsOrderCustomized)
                                    }
                                };

                                // Add resources
                                foreach (var resource in subject.ProjectSubjectResources.OrderBy(r => r.Order))
                                {
                                    var resourceNode = new TreeNodeDto
                                    {
                                        Id = $"resource_{resource.Id}",
                                        Text = resource.Title,
                                        Type = "resource",
                                        Icon = GetResourceIcon(resource.Type),
                                        State = new TreeNodeStateDto { Opened = false },
                                        Data = new TreeNodeDataDto
                                        {
                                            EntityId = resource.Id,
                                            SourceId = resource.SourceSubjectResourceId,
                                            IsCustomized = resource.IsTitleCustomized ||
resource.IsUrlCustomized ||
resource.IsTypeCustomized ||
resource.IsEstimatedMinutesCustomized ||
                                                           resource.IsOrderCustomized,
                                            CustomizationStatus = GetCustomizationStatus(
                                                resource.SourceSubjectResourceId,
                                                resource.IsTitleCustomized ||
resource.IsUrlCustomized ||
resource.IsTypeCustomized ||
resource.IsEstimatedMinutesCustomized ||
                                                resource.IsOrderCustomized)
                                        },
                                        Attributes = new Dictionary<string, string>
                                        {
                                            { "href", resource.Url },
                                            { "target", "_blank" }
                                        }
                                    };

                                    subjectNode.Children.Add(resourceNode);
                                }

                                topicNode.Children.Add(subjectNode);
                            }

                            moduleNode.Children.Add(topicNode);
                        }
                    }
                    else
                    {
                        // Module has no topics - add a placeholder message
                        var emptyNode = new TreeNodeDto
                        {
                            Id = $"empty_{module.Id}",
                            Text = "(Sin temas)",
                            Type = "empty",
                            Icon = "fas fa-info-circle text-muted",
                            State = new TreeNodeStateDto { Opened = false }
                        };
                        moduleNode.Children.Add(emptyNode);
                    }

                    rootNode.Children.Add(moduleNode);
                }
            }
            else
            {
                // No modules - add a placeholder message
                var emptyNode = new TreeNodeDto
                {
                    Id = "empty_structure",
                    Text = "(Sin módulos)",
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
                "Error getting project knowledge structure tree for project {ProjectExternalId}",
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown, ("KnowledgeStructureTree", "Error al obtener la estructura de conocimiento"));
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

    private static string GetResourceIcon(string resourceType)
    {
        return resourceType?.ToLowerInvariant() switch
        {
            "video" => "fas fa-video",
            "document" => "fas fa-file-pdf",
            "article" => "fas fa-newspaper",
            "link" => "fas fa-link",
            _ => "fas fa-external-link-alt"
        };
    }
}
