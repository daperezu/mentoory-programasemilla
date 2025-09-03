namespace LinaSys.Diagnostics.Domain.Aggregates.Form;

public partial class FormQuestion
{
    public FormQuestion(long formId, long questionId, long? topicId, long blockId, int order)
    {
        FormId = formId;
        QuestionId = questionId;
        TopicId = topicId;
        BlockId = blockId;
        Order = order;
    }

    public FormQuestion(Form form, Question question, long? topicId, long blockId, int order)
    {
        Form = form;
        Question = question;
        TopicId = topicId;
        BlockId = blockId;
        Order = order;
    }

    public FormQuestion(Form form, Question question, long? topicId, Block.Block block, int order)
    {
        Form = form;
        Question = question;
        TopicId = topicId;
        Block = block;
        Order = order;
    }

    protected FormQuestion()
    {
    }

    public long FormId { get; private set; }

    public long QuestionId { get; private set; }

    public long? TopicId { get; private set; }

    public long BlockId { get; private set; }

    public int Order { get; private set; }

    public virtual Block.Block Block { get; private set; }

    public virtual Form Form { get; private set; }

    public virtual Question Question { get; private set; }

    public void UpdateOrder(int order)
    {
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be a non-negative integer.");
        }

        Order = order;
    }
}
