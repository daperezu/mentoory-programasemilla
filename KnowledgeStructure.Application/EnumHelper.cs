namespace LinaSys.KnowledgeStructure.Application;

public static class EnumHelper
{
    public static Dictionary<int, string> ToDictionary<TEnum>()
        where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .ToDictionary(e => Convert.ToInt32(e), e => e.ToString());
    }
}
