namespace SaaSFoundry.SDK.Import.Models;

public sealed class ColumnMapping
{
    public string SourceColumn { get; }
    public string TargetField { get; }

    public ColumnMapping(string sourceColumn, string targetField)
    {
        SourceColumn = sourceColumn;
        TargetField = targetField;
    }
}
