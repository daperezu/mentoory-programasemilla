using LinaSys.Orchestration.Application.BusinessIncubator.Commands;
using LinaSys.Web.Areas.BusinessIncubators.Models.BusinessIncubator;

namespace LinaSys.Web.Services;

public static class ResultContextToViewModelMapper
{
    private static readonly Dictionary<Type, Dictionary<string, string>> _dictionary = new()
    {
        { typeof(CreateBusinessIncubatorWithPackageCommand), CreateBusinessIncubatorWithPackageCommandMap() },
    };

    public static Dictionary<string, string> GetMap<T>()
    {
        return _dictionary.TryGetValue(typeof(T), out var map) ? map : [];
    }

    private static Dictionary<string, string> CreateBusinessIncubatorWithPackageCommandMap() => new()
    {
        { nameof(CreateBusinessIncubatorWithPackageCommand.Name), nameof(CreateViewModel.Name) },
    };
}
