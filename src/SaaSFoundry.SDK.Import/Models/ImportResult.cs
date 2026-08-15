using System.Collections.Generic;
using System.Linq;

namespace SaaSFoundry.SDK.Import.Models;

public sealed class ImportResult<T>
{
    public List<ImportRecord<T>> Rows { get; } = new();
    public List<string> DetectedColumns { get; } = new();
    
    public int TotalRows => Rows.Count;
    public int ValidRows => Rows.Count(r => r.Category == ImportCategory.Valid);
    public int WarningRows => Rows.Count(r => r.Category == ImportCategory.Warning);
    public int CorrectableRows => Rows.Count(r => r.Category == ImportCategory.Correctable);
    public int InvalidRows => Rows.Count(r => r.Category == ImportCategory.Invalid);
}
