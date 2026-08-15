using System.Collections.Generic;

namespace SaaSFoundry.SDK.Import.Models;

public sealed class ImportRecord<T>
{
    public long RowNumber { get; init; }
    public T? Data { get; init; }
    public ImportCategory Category { get; set; } = ImportCategory.Valid;
    public List<ImportDiagnostic> Diagnostics { get; } = new();
}
