using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Attributes;

/// <summary>
/// Validation attribute that makes a field required based on the selected role.
/// </summary>
public class RequiredIfRoleAttribute : ValidationAttribute
{
    private readonly string[] _requiredForRoles;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredIfRoleAttribute"/> class.
    /// </summary>
    /// <param name="roles">Comma-separated list of roles that require this field.</param>
    /// <param name="errorMessage">The error message to display when validation fails.</param>
    public RequiredIfRoleAttribute(string roles, string? errorMessage = null)
    {
        _requiredForRoles = roles.Split(',').Select(r => r.Trim()).ToArray();
        ErrorMessage = errorMessage ?? "Este campo es requerido para el rol seleccionado.";
    }

    public override bool RequiresValidationContext => true;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Get the SelectedRole property from the model
        var roleProperty = validationContext.ObjectType.GetProperty("SelectedRole");
        if (roleProperty == null)
        {
            // If there's no SelectedRole property, skip validation
            return ValidationResult.Success;
        }

        var selectedRole = roleProperty.GetValue(validationContext.ObjectInstance) as string;
        if (string.IsNullOrEmpty(selectedRole))
        {
            // If no role is selected, skip validation
            return ValidationResult.Success;
        }

        // Check if the selected role requires this field
        if (_requiredForRoles.Contains(selectedRole))
        {
            // Field is required for this role
            if (value == null || (value is long longValue && longValue <= 0))
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}