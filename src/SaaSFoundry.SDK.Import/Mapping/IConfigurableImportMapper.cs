using System.Collections.Generic;
using SaaSFoundry.SDK.Import.Mapping;

namespace SaaSFoundry.SDK.Import.Mapping;

public interface IConfigurableImportMapper<T> : IImportMapper<T>
{
    IEnumerable<string> SupportedTargetFields { get; }
}
