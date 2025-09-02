namespace LinaSys.KnowledgeStructure.Application.Module.DTOs;

public sealed record ModuleDto(
    long Id,
    string Name,
    string? Description,
    long KnowledgeStructureId,
    string KnowledgeStructureName,
    int Order);

public sealed record ModuleListDto(
    long Id,
    string Name,
    long KnowledgeStructureId,
    string KnowledgeStructureName,
    int Order,
    int TopicCount);
