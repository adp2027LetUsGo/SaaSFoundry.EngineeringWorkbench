using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.SDK.Import.Models;

namespace SaaSFoundry.SDK.Import.AI.Models;

public sealed class SemanticMappingResult
{
    public IReadOnlyList<SuggestedMapping> Mappings { get; }

    public SemanticMappingResult(IReadOnlyList<SuggestedMapping> mappings)
    {
        Mappings = mappings ?? new List<SuggestedMapping>();
    }

    public ColumnMappingConfiguration ToConfiguration()
    {
        var confirmed = Mappings
            .Where(m => m.Status == MappingStatus.Confirmed && !string.IsNullOrWhiteSpace(m.TargetField))
            .Select(m => m.ToColumnMapping())
            .ToList();
            
        return new ColumnMappingConfiguration(confirmed);
    }
}
