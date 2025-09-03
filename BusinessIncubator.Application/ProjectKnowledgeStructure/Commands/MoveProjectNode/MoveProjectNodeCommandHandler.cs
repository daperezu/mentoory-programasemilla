using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.MoveProjectNode;

/// <summary>
/// Handler for moving a node in the project knowledge structure.
/// </summary>
public sealed class MoveProjectNodeCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<MoveProjectNodeCommandHandler> logger)
    : BaseCommandHandler<MoveProjectNodeCommand>
{
    public override async Task<Result> Handle(
        MoveProjectNodeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project
            var project = await repository.GetProjectByExternalIdAsync(
                request.ProjectExternalId,
                cancellationToken);

            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "Proyecto no encontrado"));
            }

            // Parse node and parent IDs
            var nodeIdParts = request.NodeId.Split('_');
            var nodeType = nodeIdParts[0];
            var nodeId = long.Parse(nodeIdParts[1]);

            // Handle different node types
            // For now, we only support reordering modules within the same parent
            switch (nodeType.ToLower())
            {
                case "module":
                    return await MoveModule(project, nodeId, request.Position, cancellationToken);

                default:
                    return Failure(ResultErrorCodes.Unknown,
                        ("NodeType", "Solo se soporta el reordenamiento de módulos temporalmente"));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error moving node {NodeId} for project {ProjectId}",
                request.NodeId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("MoveNode", "Error al mover el elemento"));
        }
    }

    private async Task<Result> MoveModule(
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.Project project,
        long moduleId,
        int position,
        CancellationToken cancellationToken)
    {
        var knowledgeStructure = await repository
            .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

        if (knowledgeStructure is null)
        {
            return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                ("KnowledgeStructure", "El proyecto no tiene estructura de conocimiento"));
        }

        var module = knowledgeStructure.ProjectModules.FirstOrDefault(m => m.Id == moduleId);
        if (module is null)
        {
            return Failure(ResultErrorCodes.Module_NotFound, ("ModuleId", "Módulo no encontrado"));
        }

        // Update order
        module.UpdateOrder(position, isOrderCustomized: true);

        // Reorder other modules
        var orderedModules = knowledgeStructure.ProjectModules
            .Where(m => m.Id != moduleId)
            .OrderBy(m => m.Order)
            .ToList();

        var currentPosition = 1;
        foreach (var m in orderedModules)
        {
            if (currentPosition == position)
            {
                currentPosition++;
            }

            if (m.Order != currentPosition)
            {
                m.UpdateOrder(currentPosition, isOrderCustomized: true);
            }

            currentPosition++;
        }

        repository.Update(project);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }

    /* Temporarily disabled - not supported yet
    private async Task<Result> MoveTopic(
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.Project project,
        long topicId,
        long newModuleId,
        int position,
        CancellationToken cancellationToken)
    {
        var knowledgeStructure = await repository
            .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

        if (knowledgeStructure is null)
            return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                ("KnowledgeStructure", "El proyecto no tiene estructura de conocimiento"));

        // Find topic and its current module
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectTopic? topic = null;
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectModule? currentModule = null;
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectModule? newModule = null;

        foreach (var module in knowledgeStructure.ProjectModules)
        {
            if (module.Id == newModuleId)
                newModule = module;

            var foundTopic = module.ProjectTopics.FirstOrDefault(t => t.Id == topicId);
            if (foundTopic is not null)
            {
                topic = foundTopic;
                currentModule = module;
            }
        }

        if (topic is null || currentModule is null)
            return Failure(ResultErrorCodes.Topic_NotFound, ("TopicId", "Tema no encontrado"));

        if (newModule is null)
            return Failure(ResultErrorCodes.Module_NotFound, ("ModuleId", "Módulo destino no encontrado"));

        // If moving to a different module
        if (currentModule.Id != newModuleId)
        {
            // Moving topics between modules is not supported yet
            return Failure(ResultErrorCodes.Unknown,
                ("Move", "Mover temas entre módulos no está soportado aún"));
        }

        // Update order
        topic.UpdateOrder(position, isCustomized: true);

        // Reorder topics in the target module
        var orderedTopics = newModule.ProjectTopics
            .Where(t => t.Id != topicId)
            .OrderBy(t => t.Order)
            .ToList();

        var currentPosition = 1;
        foreach (var t in orderedTopics)
        {
            if (currentPosition == position)
                currentPosition++;

            if (t.Order != currentPosition)
                t.UpdateOrder(currentPosition, isCustomized: true);

            currentPosition++;
        }

        repository.Update(project);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }

    private async Task<Result> MoveSubject(
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.Project project,
        long subjectId,
        long newTopicId,
        int position,
        CancellationToken cancellationToken)
    {
        var knowledgeStructure = await repository
            .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

        if (knowledgeStructure is null)
            return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                ("KnowledgeStructure", "El proyecto no tiene estructura de conocimiento"));

        // Find subject and its current topic
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectSubject? subject = null;
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectTopic? currentTopic = null;
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectTopic? newTopic = null;

        foreach (var module in knowledgeStructure.ProjectModules)
        {
            foreach (var topic in module.ProjectTopics)
            {
                if (topic.Id == newTopicId)
                    newTopic = topic;

                var foundSubject = topic.ProjectSubjects.FirstOrDefault(s => s.Id == subjectId);
                if (foundSubject is not null)
                {
                    subject = foundSubject;
                    currentTopic = topic;
                }
            }
        }

        if (subject is null || currentTopic is null)
            return Failure(ResultErrorCodes.Subject_NotFound, ("SubjectId", "Materia no encontrada"));

        if (newTopic is null)
            return Failure(ResultErrorCodes.Topic_NotFound, ("TopicId", "Tema destino no encontrado"));

        // If moving to a different topic
        if (currentTopic.Id != newTopicId)
        {
            // Moving subjects between topics is not supported yet
            return Failure(ResultErrorCodes.Unknown,
                ("Move", "Mover materias entre temas no está soportado aún"));
        }

        // Update order
        subject.UpdateOrder(position, isCustomized: true);

        // Reorder subjects in the target topic
        var orderedSubjects = newTopic.ProjectSubjects
            .Where(s => s.Id != subjectId)
            .OrderBy(s => s.Order)
            .ToList();

        var currentPosition = 1;
        foreach (var s in orderedSubjects)
        {
            if (currentPosition == position)
                currentPosition++;

            if (s.Order != currentPosition)
                s.UpdateOrder(currentPosition, isCustomized: true);

            currentPosition++;
        }

        repository.Update(project);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }

    private async Task<Result> MoveBlock(
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.Project project,
        long blockId,
        int position,
        CancellationToken cancellationToken)
    {
        // Get project with blocks
        var projectWithBlocks = await repository.GetProjectWithBlocksByExternalIdAsync(
            project.ExternalId,
            cancellationToken);

        if (projectWithBlocks is null)
            return Failure(ResultErrorCodes.Project_NotFound, ("ProjectId", "Proyecto no encontrado"));

        var block = projectWithBlocks.ProjectBlocks.FirstOrDefault(b => b.Id == blockId);
        if (block is null)
            return Failure(ResultErrorCodes.Block_NotFound, ("BlockId", "Bloque no encontrado"));

        // Update order
        block.UpdateOrder(position, isCustomized: true);

        // Reorder other blocks
        var orderedBlocks = projectWithBlocks.ProjectBlocks
            .Where(b => b.Id != blockId)
            .OrderBy(b => b.Order)
            .ToList();

        var currentPosition = 1;
        foreach (var b in orderedBlocks)
        {
            if (currentPosition == position)
                currentPosition++;

            if (b.Order != currentPosition)
                b.UpdateOrder(currentPosition, isCustomized: true);

            currentPosition++;
        }

        repository.Update(projectWithBlocks);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }

    private async Task<Result> MoveQuestion(
        LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.Project project,
        long questionId,
        string parentType,
        long parentId,
        int position,
        CancellationToken cancellationToken)
    {
        // Get project with questions
        var projectWithQuestions = await repository.GetProjectWithQuestionsByExternalIdAsync(
            project.ExternalId,
            cancellationToken);

        if (projectWithQuestions is null)
            return Failure(ResultErrorCodes.Project_NotFound, ("ProjectId", "Proyecto no encontrado"));

        var question = projectWithQuestions.ProjectQuestions.FirstOrDefault(q => q.Id == questionId);
        if (question is null)
            return Failure(ResultErrorCodes.Question_NotFound, ("QuestionId", "Pregunta no encontrada"));

        // Handle movement based on parent type
        if (parentType.ToLower() == "block")
        {
            // Moving to a block (no topic)
            question.UpdateProjectBlockId(parentId);
            question.UpdateProjectTopicId(null);
        }
        else if (parentType.ToLower() == "topic")
        {
            // Moving to a topic
            var knowledgeStructure = await repository
                .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

            if (knowledgeStructure is null)
                return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                    ("KnowledgeStructure", "El proyecto no tiene estructura de conocimiento"));

            // Find the topic and its block
            var topic = knowledgeStructure.ProjectModules
                .SelectMany(m => m.ProjectTopics)
                .FirstOrDefault(t => t.Id == parentId);

            if (topic is null)
                return Failure(ResultErrorCodes.Topic_NotFound, ("TopicId", "Tema no encontrado"));

            // Find which block this topic belongs to (through module)
            // This is a simplified approach - in reality, you might need to track block-module relationships
            question.UpdateProjectTopicId(parentId);
        }

        // Update order
        question.UpdateOrder(position, isCustomized: true);

        // Reorder other questions in the same parent
        var siblingQuestions = projectWithQuestions.ProjectQuestions
            .Where(q => q.Id != questionId);

        if (parentType.ToLower() == "block")
        {
            siblingQuestions = siblingQuestions.Where(q => q.ProjectBlockId == parentId && !q.ProjectTopicId.HasValue);
        }
        else if (parentType.ToLower() == "topic")
        {
            siblingQuestions = siblingQuestions.Where(q => q.ProjectTopicId == parentId);
        }

        var orderedQuestions = siblingQuestions.OrderBy(q => q.Order).ToList();

        var currentPosition = 1;
        foreach (var q in orderedQuestions)
        {
            if (currentPosition == position)
                currentPosition++;

            if (q.Order != currentPosition)
                q.UpdateOrder(currentPosition, isCustomized: true);

            currentPosition++;
        }

        repository.Update(projectWithQuestions);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
    */
}