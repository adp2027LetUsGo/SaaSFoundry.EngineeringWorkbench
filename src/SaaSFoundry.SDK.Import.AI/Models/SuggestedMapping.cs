using SaaSFoundry.SDK.AI.Models;
using SaaSFoundry.SDK.Import.Models;

namespace SaaSFoundry.SDK.Import.AI.Models;

public sealed class SuggestedMapping
{
    public string SourceColumn { get; set; }
    public string? TargetField { get; set; }
    public MappingStatus Status { get; set; }
    public AIConfidence Confidence { get; set; }
    public string Evidence { get; set; }

    public SuggestedMapping(string sourceColumn, string? targetField, MappingStatus status, AIConfidence confidence, string evidence)
    {
        SourceColumn = sourceColumn;
        TargetField = targetField;
        Status = status;
        Confidence = confidence;
        Evidence = evidence ?? string.Empty;
    }

    public ColumnMapping ToColumnMapping()
    {
        if (Status != MappingStatus.Confirmed || string.IsNullOrWhiteSpace(TargetField))
        {
            throw new System.InvalidOperationException($"Mapping for {SourceColumn} cannot be exported unless confirmed and target is specified.");
        }
        return new ColumnMapping(SourceColumn, TargetField);
    }
}
