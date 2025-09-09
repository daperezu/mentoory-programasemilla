using LinaSys.Shared.Application;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.UserManagement.Domain.ValueObjects;

namespace LinaSys.UserManagement.Domain.AggregatesModel.UserProfileAggregate;

public class UserProfile : AuditableEntity, IAggregateRoot
{
    private readonly List<UserPreferences> _preferences = new();
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _identification = string.Empty;
    private Location? _location;
    private string? _avatarUrl;
    private bool _isActive = true;

    protected UserProfile()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string FirstName
    {
        get => _firstName;
        private set => _firstName = value ?? string.Empty;
    }

    public string LastName
    {
        get => _lastName;
        private set => _lastName = value ?? string.Empty;
    }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Identification
    {
        get => _identification;
        private set => _identification = value ?? string.Empty;
    }

    public Location? Location => _location;

    public string? AvatarUrl => _avatarUrl;

    public bool IsActive => _isActive;

    public IReadOnlyCollection<UserPreferences> Preferences => _preferences.AsReadOnly();

    public static UserProfile Create(
        string userId,
        string firstName,
        string lastName,
        string identification,
        IAuditContext auditContext)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId is required", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("FirstName is required", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("LastName is required", nameof(lastName));
        }

        if (string.IsNullOrWhiteSpace(identification))
        {
            throw new ArgumentException("Identification is required", nameof(identification));
        }

        var profile = new UserProfile
        {
            UserId = userId,
            _firstName = firstName.Trim(),
            _lastName = lastName.Trim(),
            _identification = identification.Trim(),
            _isActive = true
        };

        profile.SetCreated(auditContext);
        return profile;
    }

    public Result UpdateProfile(
        string firstName,
        string lastName,
        IAuditContext auditContext)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure(ResultErrorCodes.ValidationError, ("UserProfile", "FirstName is required"));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure(ResultErrorCodes.ValidationError, ("UserProfile", "LastName is required"));
        }

        if (FirstName == firstName.Trim() && LastName == lastName.Trim())
        {
            return Result.Success();
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        SetUpdated(auditContext);

        return Result.Success();
    }

    public Result UpdateLocation(
        string? country,
        string? province,
        string? canton,
        string? district,
        string? fullAddress,
        IAuditContext auditContext)
    {
        // Delegate all validation to the Location value object
        var locationResult = Location.Create(country, province, canton, district, fullAddress);
        if (!locationResult.IsSuccess)
        {
            return Result.Failure(locationResult.ErrorCode ?? ResultErrorCodes.ValidationError, locationResult.ErrorMessages ?? [("UserProfile", "Invalid location")]);
        }

        _location = locationResult.Value;
        SetUpdated(auditContext);

        return Result.Success();
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        _avatarUrl = avatarUrl;
    }

    public void Deactivate()
    {
        _isActive = false;
    }

    public void Reactivate()
    {
        _isActive = true;
    }

    public Result AddOrUpdatePreference(
        string key,
        string value,
        IAuditContext auditContext)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Result.Failure(ResultErrorCodes.ValidationError, ("UserProfile", "Preference key is required"));
        }

        var existing = _preferences.FirstOrDefault(p => p.Key == key);
        if (existing is not null)
        {
            existing.UpdateValue(value);
        }
        else
        {
            _preferences.Add(UserPreferences.Create(key, value));
        }

        SetUpdated(auditContext);
        return Result.Success();
    }
}