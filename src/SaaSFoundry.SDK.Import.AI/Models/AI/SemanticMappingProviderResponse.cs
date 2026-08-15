using System.Collections.Generic;
namespace SaaSFoundry.SDK.Import.AI.Models.AI;

public sealed class SemanticMappingProviderResponse
{
    public IReadOnlyList<ProviderSuggestedMapping> Suggestions { get; set; } = new List<ProviderSuggestedMapping>();
}

public sealed class ProviderSuggestedMapping
{
    public string SourceColumn { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
}
