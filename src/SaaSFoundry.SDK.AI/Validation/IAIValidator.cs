namespace SaaSFoundry.SDK.AI.Validation;

public interface IAIValidator<TOutput>
{
    ValidationResult Validate(TOutput output);
}
