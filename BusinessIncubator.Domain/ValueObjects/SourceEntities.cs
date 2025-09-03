namespace LinaSys.BusinessIncubator.Domain.ValueObjects;

/// <summary>
/// Placeholder class representing a source KnowledgeStructure for synchronization.
/// These would typically come from other bounded contexts (Diagnostics, KnowledgeStructure).
/// </summary>
public class KnowledgeStructure
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Placeholder class representing a source Module for synchronization.
/// </summary>
public class Module
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

/// <summary>
/// Placeholder class representing a source Topic for synchronization.
/// </summary>
public class Topic
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

/// <summary>
/// Placeholder class representing a source Subject for synchronization.
/// </summary>
public class Subject
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

/// <summary>
/// Placeholder class representing a source Question for synchronization.
/// </summary>
public class Question
{
    public long Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? HelpText { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public string AnswerType { get; set; } = string.Empty;
    public List<AnswerOption>? AnswerOptions { get; set; }
}

/// <summary>
/// Placeholder class representing a source AnswerOption for synchronization.
/// </summary>
public class AnswerOption
{
    public long Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int? Value { get; set; }
    public int Order { get; set; }
}