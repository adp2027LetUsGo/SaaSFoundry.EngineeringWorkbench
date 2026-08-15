namespace SaaSFoundry.SDK.AI.Validation;

public sealed class ValidationResult
{
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    private ValidationResult(bool isValid, string errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public static ValidationResult Success() => new(true, string.Empty);
    public static ValidationResult Failure(string message) => new(false, message);
}
