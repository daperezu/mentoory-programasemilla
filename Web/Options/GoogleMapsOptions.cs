namespace LinaSys.Web.Options;

/// <summary>
/// Configuration options for Google Maps integration.
/// </summary>
public sealed class GoogleMapsOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "GoogleMaps";

    /// <summary>
    /// Gets the Google Maps API key.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets the default map language (e.g., "es" for Spanish).
    /// </summary>
    public string Language { get; init; } = "es";

    /// <summary>
    /// Gets the default map region for biasing results (e.g., "CR" for Costa Rica).
    /// </summary>
    public string Region { get; init; } = "CR";

    /// <summary>
    /// Gets a value indicating whether gets or sets whether to enable the Places API library.
    /// </summary>
    public bool EnablePlacesLibrary { get; init; } = true;

    /// <summary>
    /// Gets the default zoom level for maps.
    /// </summary>
    public int DefaultZoom { get; init; } = 15;

    /// <summary>
    /// Gets a value indicating whether validates if the API key is configured.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "YOUR_DEVELOPMENT_GOOGLE_MAPS_API_KEY";
}