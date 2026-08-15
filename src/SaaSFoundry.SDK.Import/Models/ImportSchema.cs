using System.Collections.Generic;

namespace SaaSFoundry.SDK.Import.Models;

public sealed class ImportSchema
{
    public IReadOnlyList<string> Columns { get; }

    public ImportSchema(IReadOnlyList<string> columns)
    {
        Columns = columns;
    }
}
