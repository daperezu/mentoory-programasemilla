using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.UserManagement.Domain.AggregatesModel.UserProfileAggregate;

public class UserPreferences : Entity
{
    private string _key = string.Empty;
    private string _value = string.Empty;

    protected UserPreferences()
    {
    }

    public string Key
    {
        get => _key;
        private set => _key = value ?? string.Empty;
    }

    public string Value
    {
        get => _value;
        private set => _value = value ?? string.Empty;
    }

    public static UserPreferences Create(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key is required", nameof(key));
        }

        return new UserPreferences
        {
            _key = key.Trim(),
            _value = value?.Trim() ?? string.Empty
        };
    }

    public void UpdateValue(string value)
    {
        _value = value?.Trim() ?? string.Empty;
    }
}