namespace LinaSys.BusinessIncubator.Domain.Aggregates.Starter;

public static class ResourceCategories
{
    public const string Guide = "guide";
    public const string Template = "template";
    public const string Video = "video";
    public const string Article = "article";
    public const string Tool = "tool";
    public const string Document = "document";
    public const string Course = "course";
    public const string Example = "example";
}

public static class ResourceTypes
{
    public const string Pdf = "pdf";
    public const string Word = "doc";
    public const string Excel = "xls";
    public const string PowerPoint = "ppt";
    public const string Video = "video";
    public const string Link = "link";
    public const string Youtube = "youtube";
    public const string Template = "template";
}

public class StarterResource
{
    public StarterResource(
        long projectId,
        string category,
        string title,
        string description,
        string resourceType,
        string phase,
        string createdBy,
        bool isRequired = false)
    {
        ProjectId = projectId;
        Category = category;
        Title = title;
        Description = description;
        ResourceType = resourceType;
        Phase = phase;
        CreatedBy = createdBy;
        IsRequired = isRequired;
        IsActive = true;
        CreatedDate = DateTime.UtcNow;
        ViewCount = 0;
        Order = 0;
    }

    protected StarterResource()
    {
    }

    public long Id { get; private set; }
    public long ProjectId { get; private set; }
    public string Category { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string ResourceType { get; private set; }
    public string? Url { get; private set; }
    public string? FilePath { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public string Phase { get; private set; }
    public int Order { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // Resource access tracking
    public int ViewCount { get; private set; }
    public DateTime? LastViewedDate { get; private set; }
    public string? LastViewedBy { get; private set; }

    public void UpdateDetails(string title, string description, string category)
    {
        Title = title;
        Description = description;
        Category = category;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void SetUrl(string url, string? thumbnailUrl = null)
    {
        Url = url;
        ThumbnailUrl = thumbnailUrl;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void SetFilePath(string filePath)
    {
        FilePath = filePath;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void SetOrder(int order)
    {
        Order = order;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void MarkAsRequired()
    {
        IsRequired = true;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void MarkAsOptional()
    {
        IsRequired = false;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void RecordView(string userId)
    {
        ViewCount++;
        LastViewedDate = DateTime.UtcNow;
        LastViewedBy = userId;
    }

    public bool IsDocument()
    {
        return ResourceType?.ToLower() is "pdf" or "doc" or "docx" or "xls" or "xlsx" or "ppt" or "pptx";
    }

    public bool IsVideo()
    {
        return ResourceType?.ToLower() is "video" or "mp4" or "avi" or "mov" or "youtube" or "vimeo";
    }

    public bool IsLink()
    {
        return ResourceType?.ToLower() is "link" or "url" or "website";
    }

    public bool IsTemplate()
    {
        return ResourceType?.ToLower() is "template" or "form";
    }
}