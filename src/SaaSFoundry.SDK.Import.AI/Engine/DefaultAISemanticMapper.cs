using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.SDK.AI;
using SaaSFoundry.SDK.AI.Models;
using SaaSFoundry.SDK.Import.Models;
using SaaSFoundry.SDK.Import.AI.Models;
using SaaSFoundry.SDK.Import.AI.Models.AI;
using SaaSFoundry.SDK.Import.AI.Validation;

namespace SaaSFoundry.SDK.Import.AI.Engine;

public sealed class DefaultAISemanticMapper : IAISemanticMapper
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultAISemanticMapper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<SemanticMappingResult> SuggestMappingsAsync(ImportSchema schema, IEnumerable<string> allowedFields, IReadOnlyDictionary<string, string>? aliases, CancellationToken cancellationToken = default)
    {
        var suggestions = new List<SuggestedMapping>();
        var allowedSet = new HashSet<string>(allowedFields, StringComparer.OrdinalIgnoreCase);
        var unresolvedColumns = new List<string>();

        var safeAliases = aliases ?? new Dictionary<string, string>();

        // 1. Deterministic First
        foreach (var column in schema.Columns)
        {
            if (allowedSet.Contains(column))
            {
                suggestions.Add(new SuggestedMapping(column, column, MappingStatus.Confirmed, AIConfidence.High, "Exact match"));
                continue;
            }

            var normalized = column.Replace(" ", "").Replace("_", "").Replace("-", "");
            var normalizedMatch = allowedSet.FirstOrDefault(f => f.Replace(" ", "").Replace("_", "").Replace("-", "").Equals(normalized, StringComparison.OrdinalIgnoreCase));
            if (normalizedMatch != null)
            {
                suggestions.Add(new SuggestedMapping(column, normalizedMatch, MappingStatus.Confirmed, AIConfidence.High, "Normalized match"));
                continue;
            }

            if (safeAliases.TryGetValue(column, out var aliasTarget) && allowedSet.Contains(aliasTarget))
            {
                suggestions.Add(new SuggestedMapping(column, aliasTarget, MappingStatus.Confirmed, AIConfidence.High, "Alias match"));
                continue;
            }

            unresolvedColumns.Add(column);
        }

        if (unresolvedColumns.Count == 0)
        {
            return new SemanticMappingResult(suggestions);
        }

        // 2. Filter remaining allowed fields (already mapped targets should not be mapped again generally)
        var takenTargets = new HashSet<string>(suggestions.Select(s => s.TargetField!), StringComparer.OrdinalIgnoreCase);
        var remainingTargets = allowedSet.Where(t => !takenTargets.Contains(t)).ToList();

        if (remainingTargets.Count == 0)
        {
            foreach (var col in unresolvedColumns)
            {
                suggestions.Add(new SuggestedMapping(col, null, MappingStatus.Unresolved, AIConfidence.Unknown, "No remaining targets."));
            }
            return new SemanticMappingResult(suggestions);
        }

        // 3. Invoke AI
        var aiEngine = _serviceProvider.GetService<IAIEngine>();
        if (aiEngine == null)
        {
            foreach (var col in unresolvedColumns)
            {
                suggestions.Add(new SuggestedMapping(col, null, MappingStatus.Unresolved, AIConfidence.Unknown, "AI Engine unavailable."));
            }
            return new SemanticMappingResult(suggestions);
        }

        var prompt = new SemanticMappingPrompt
        {
            UnresolvedColumns = unresolvedColumns,
            AllowedTargetFields = remainingTargets
        };

        var request = new AIRequest<SemanticMappingPrompt>(prompt, "SemanticColumnMapping", TimeSpan.FromSeconds(30));
        
        // Scope the validator for this specific request
        var validator = new SemanticMappingValidator(remainingTargets);
        var engineScope = _serviceProvider.CreateScope();
        
        // We simulate injecting a temporary validator using a wrapped engine call for tests, 
        // since the framework IAIEngine pulls IAIValidator<T> from DI. 
        // Actually, NativeAOT requires static DI. So we just bypass the AI engine's automatic DI validator and run it manually for now, 
        // OR the provider returns the response and we validate it in the Mapper.
        // The instructions say "Reuse the certified Stage 4A/4B validation mechanism".
        // The Engine in 4B pulls `IAIValidator<T>` from DI. Because the allowed targets are dynamic per request, 
        // a scoped DI registration is problematic. We will call the engine without validator in DI, and validate manually here if it succeeds.
        
        var aiResult = await aiEngine.ExecuteAsync<SemanticMappingPrompt, SemanticMappingProviderResponse>(request, cancellationToken);
        
        if (aiResult.Status == AIResultStatus.Success && aiResult.Response != null)
        {
            var valResult = validator.Validate(aiResult.Response.Output);
            if (!valResult.IsValid)
            {
                foreach (var col in unresolvedColumns)
                {
                    suggestions.Add(new SuggestedMapping(col, null, MappingStatus.Rejected, AIConfidence.Unknown, $"AI Validation Failed: {valResult.ErrorMessage}"));
                }
                return new SemanticMappingResult(suggestions);
            }

            foreach (var col in unresolvedColumns)
            {
                var aiMatch = aiResult.Response.Output.Suggestions.FirstOrDefault(s => string.Equals(s.SourceColumn, col, StringComparison.OrdinalIgnoreCase));
                if (aiMatch != null && !string.IsNullOrWhiteSpace(aiMatch.TargetField))
                {
                    var conf = Enum.TryParse<AIConfidence>(aiMatch.Confidence, true, out var parsedConf) ? parsedConf : AIConfidence.Unknown;
                    suggestions.Add(new SuggestedMapping(col, aiMatch.TargetField, MappingStatus.Suggested, conf, aiMatch.Evidence));
                }
                else
                {
                    suggestions.Add(new SuggestedMapping(col, null, MappingStatus.Unresolved, AIConfidence.Unknown, "AI provided no suggestion."));
                }
            }
        }
        else
        {
            // Timeout, ProviderError, etc.
            foreach (var col in unresolvedColumns)
            {
                suggestions.Add(new SuggestedMapping(col, null, MappingStatus.Unresolved, AIConfidence.Unknown, $"AI Error: {aiResult.Status} - {aiResult.ErrorMessage}"));
            }
        }

        return new SemanticMappingResult(suggestions);
    }
}
