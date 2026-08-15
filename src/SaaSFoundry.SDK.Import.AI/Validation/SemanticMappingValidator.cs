using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.SDK.AI.Validation;
using SaaSFoundry.SDK.Import.AI.Models.AI;

namespace SaaSFoundry.SDK.Import.AI.Validation;

public sealed class SemanticMappingValidator : IAIValidator<SemanticMappingProviderResponse>
{
    private readonly IEnumerable<string> _allowedTargets;

    public SemanticMappingValidator(IEnumerable<string> allowedTargets)
    {
        _allowedTargets = allowedTargets;
    }

    public ValidationResult Validate(SemanticMappingProviderResponse output)
    {
        if (output == null || output.Suggestions == null)
            return ValidationResult.Failure("Output or suggestions list is null.");

        var allowedSet = new HashSet<string>(_allowedTargets, StringComparer.OrdinalIgnoreCase);
        
        foreach (var suggestion in output.Suggestions)
        {
            if (string.IsNullOrWhiteSpace(suggestion.SourceColumn))
                return ValidationResult.Failure("A suggestion is missing a SourceColumn.");
                
            if (!string.IsNullOrWhiteSpace(suggestion.TargetField) && !allowedSet.Contains(suggestion.TargetField))
                return ValidationResult.Failure($"Suggested target field '{suggestion.TargetField}' is not in the allowed targets list.");
        }

        var duplicates = output.Suggestions
            .Where(s => !string.IsNullOrWhiteSpace(s.TargetField))
            .GroupBy(s => s.TargetField, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
            return ValidationResult.Failure($"Duplicate targets suggested by AI: {string.Join(", ", duplicates)}");

        return ValidationResult.Success();
    }
}
