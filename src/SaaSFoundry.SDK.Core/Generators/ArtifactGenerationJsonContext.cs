using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SaaSFoundry.SDK.Core.Generators;

[JsonSerializable(typeof(IReadOnlyList<GeneratedArtifactDescriptor>))]
public partial class ArtifactGenerationJsonContext : JsonSerializerContext
{
}
