namespace LinaSys.Web.Services;

public class GoogleAnalyticsService : IGoogleAnalyticsService
{
    private readonly IConfiguration _configuration;
    private readonly string? _measurementId;
    private readonly bool _enabled;

    public GoogleAnalyticsService(IConfiguration configuration)
    {
        _configuration = configuration;
        _measurementId = _configuration["GoogleAnalytics:MeasurementId"];
        _enabled = bool.TryParse(_configuration["GoogleAnalytics:Enabled"], out var enabled) && enabled;
    }

    public string? GetMeasurementId() => _measurementId;

    public bool IsEnabled() => _enabled && !string.IsNullOrWhiteSpace(_measurementId);

    public string GetGtagScriptUrl() => $"https://www.googletagmanager.com/gtag/js?id={_measurementId}";

    public string GetInitializationScript()
    {
        if (!IsEnabled())
        {
            return string.Empty;
        }

        return $@"
        window.dataLayer = window.dataLayer || [];
        function gtag(){{dataLayer.push(arguments);}}
        gtag('js', new Date());
        gtag('config', '{_measurementId}');";
    }
}