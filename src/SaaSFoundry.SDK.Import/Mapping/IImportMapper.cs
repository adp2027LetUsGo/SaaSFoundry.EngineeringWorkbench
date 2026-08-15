using System.Collections.Generic;

namespace SaaSFoundry.SDK.Import.Mapping;

public interface IImportMapper<T>
{
    T Map(IReadOnlyDictionary<string, string> row);
}
