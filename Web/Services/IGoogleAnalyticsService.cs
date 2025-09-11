namespace LinaSys.Web.Services;

public interface IGoogleAnalyticsService
{
    string? GetMeasurementId();
    bool IsEnabled();
    string GetGtagScriptUrl();
    string GetInitializationScript();
}