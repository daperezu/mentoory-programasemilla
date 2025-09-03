namespace LinaSys.Web.Models.Shared;

public enum ColFilterType
{
    Default,
    Custom,
    Select,
}

public enum ColRenderType
{
    Default,
    Custom,
    Link,
    Badge,
}

public class DatatableColumn
{
    public Dictionary<string, string> BadgeClassMap { get; set; } = [];

    public Dictionary<string, string> BadgeMap { get; set; } = [];

    public string Data { get; set; } = string.Empty;

    public string FilterJs { get; set; } = string.Empty;

    public Dictionary<string, string> FilterOptions { get; set; } = [];

    public ColFilterType FilterType { get; set; } = ColFilterType.Default;

    public string Header { get; set; } = string.Empty;

    public string LinkTextField { get; set; } = string.Empty;

    public string LinkUrl { get; set; } = string.Empty;

    public bool Orderable { get; set; } = true;

    public string RenderJs { get; set; } = string.Empty;

    public ColRenderType RenderType { get; set; } = ColRenderType.Default;

    public bool Searchable { get; set; } = true;
}
