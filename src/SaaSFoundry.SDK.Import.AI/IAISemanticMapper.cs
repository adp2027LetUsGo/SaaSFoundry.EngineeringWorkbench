using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.SDK.Import.Models;
using SaaSFoundry.SDK.Import.AI.Models;

namespace SaaSFoundry.SDK.Import.AI;

public interface IAISemanticMapper
{
    Task<SemanticMappingResult> SuggestMappingsAsync(ImportSchema schema, IEnumerable<string> allowedFields, IReadOnlyDictionary<string, string>? aliases, CancellationToken cancellationToken = default);
}
