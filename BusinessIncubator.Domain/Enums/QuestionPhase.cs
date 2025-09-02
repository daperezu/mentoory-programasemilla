namespace LinaSys.BusinessIncubator.Domain.Enums;

[Flags]
public enum QuestionPhase
{
    None = 0,
    Start = 1,
    Final = 2,
    Undefined = 4,
    Both = Start | Final,
}
