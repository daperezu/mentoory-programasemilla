using System.Reflection;

namespace LinaSys.Web.Services;

public interface IVersionProvider
{
    string Version { get; }
}

public class VersionProvider : IVersionProvider
{
    public VersionProvider()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        Version = assembly
                      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                  ?? assembly.GetName().Version?.ToString()
                  ?? "Unknown";
    }

    public string Version { get; }
}
