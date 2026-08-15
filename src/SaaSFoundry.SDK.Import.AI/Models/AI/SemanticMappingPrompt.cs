using System.Collections.Generic;
namespace SaaSFoundry.SDK.Import.AI.Models.AI;

public sealed class SemanticMappingPrompt
{
    public IReadOnlyList<string> UnresolvedColumns { get; set; } = new List<string>();
    public IReadOnlyList<string> AllowedTargetFields { get; set; } = new List<string>();
}
