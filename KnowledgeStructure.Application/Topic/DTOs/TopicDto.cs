namespace LinaSys.KnowledgeStructure.Application.Topic.DTOs;

public sealed record TopicDto(
    long StructureTopicId,
    long TopicId,
    string Name,
    string? Description,
    long StructureModuleId,
    string ModuleName,
    long KnowledgeStructureId,
    string KnowledgeStructureName);

public sealed record TopicListDto(
    long StructureTopicId,
    long TopicId,
    string Name,
    string? Description,
    string ModuleName,
    string KnowledgeStructureName,
    int SubjectCount);
