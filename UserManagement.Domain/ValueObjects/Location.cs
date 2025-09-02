using LinaSys.Shared.Application;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.UserManagement.Domain.ValueObjects;

public class Location : ValueObject
{
    private Location(
        string? country,
        string? province,
        string? canton,
        string? district,
        string? fullAddress)
    {
        Country = country;
        Province = province;
        Canton = canton;
        District = district;
        FullAddress = fullAddress;
    }

    public string? Country { get; }
    public string? Province { get; }
    public string? Canton { get; }
    public string? District { get; }
    public string? FullAddress { get; }

    public static Result<Location> Create(
        string? country,
        string? province,
        string? canton,
        string? district,
        string? fullAddress)
    {
        // Validate country-specific requirements
        var countryValidation = ValidateCountryRequirements(country, province, canton, district);
        if (!countryValidation.IsSuccess)
        {
            return Result<Location>.Failure(countryValidation.ErrorCode ?? ResultErrorCodes.ValidationError, countryValidation.ErrorMessages!);
        }

        // Validate field lengths
        if (country?.Length > 100)
        {
            return Result<Location>.Failure(ResultErrorCodes.ValidationError, ("Location", "El país no puede exceder 100 caracteres"));
        }

        if (province?.Length > 100)
        {
            return Result<Location>.Failure(ResultErrorCodes.ValidationError, ("Location", "La provincia no puede exceder 100 caracteres"));
        }

        if (canton?.Length > 100)
        {
            return Result<Location>.Failure(ResultErrorCodes.ValidationError, ("Location", "El cantón no puede exceder 100 caracteres"));
        }

        if (district?.Length > 100)
        {
            return Result<Location>.Failure(ResultErrorCodes.ValidationError, ("Location", "El distrito no puede exceder 100 caracteres"));
        }

        if (fullAddress?.Length > 500)
        {
            return Result<Location>.Failure(ResultErrorCodes.ValidationError, ("Location", "La dirección no puede exceder 500 caracteres"));
        }

        var location = new Location(
            country?.Trim(),
            province?.Trim(),
            canton?.Trim(),
            district?.Trim(),
            fullAddress?.Trim());

        return Result<Location>.Success(location);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Country;
        yield return Province;
        yield return Canton;
        yield return District;
        yield return FullAddress;
    }

    private static Result ValidateCountryRequirements(string? country, string? province, string? canton, string? district)
    {
        // This can be extended with a strategy pattern or configuration-based validation
        // as more countries are added
        return country?.ToUpperInvariant() switch
        {
            "COSTA RICA" => ValidateCostaRicaRequirements(province, canton, district),
            // Future countries can be added here
            // "PANAMA" => ValidatePanamaRequirements(province, canton),
            _ => Result.Success() // No specific requirements for other countries
        };
    }

    private static Result ValidateCostaRicaRequirements(string? province, string? canton, string? district)
    {
        if (string.IsNullOrWhiteSpace(province))
        {
            return Result.Failure(ResultErrorCodes.ValidationError, ("Location", "La provincia es requerida para Costa Rica"));
        }

        if (string.IsNullOrWhiteSpace(canton))
        {
            return Result.Failure(ResultErrorCodes.ValidationError, ("Location", "El cantón es requerido para Costa Rica"));
        }

        if (string.IsNullOrWhiteSpace(district))
        {
            return Result.Failure(ResultErrorCodes.ValidationError, ("Location", "El distrito es requerido para Costa Rica"));
        }

        return Result.Success();
    }
}