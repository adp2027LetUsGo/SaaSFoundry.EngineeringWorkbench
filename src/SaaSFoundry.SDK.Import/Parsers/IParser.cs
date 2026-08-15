using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SaaSFoundry.SDK.Import.Parsers;

public interface IParser
{
    IAsyncEnumerable<IReadOnlyDictionary<string, string>> ParseAsync(Stream stream, CancellationToken cancellationToken);
    List<string> Columns { get; }
}
