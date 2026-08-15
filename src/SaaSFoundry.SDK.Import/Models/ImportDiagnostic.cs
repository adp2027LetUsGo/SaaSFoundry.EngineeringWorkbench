namespace SaaSFoundry.SDK.Import.Models;

public sealed record ImportDiagnostic
{
    public ImportCategory Category { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Field { get; init; }
}
