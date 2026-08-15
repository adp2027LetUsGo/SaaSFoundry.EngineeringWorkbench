using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.SDK.Import.Models;

namespace SaaSFoundry.SDK.Import;

public interface IImportEngine
{
    Task<ImportSchema> InspectAsync(Stream stream, ImportFormat format, CancellationToken cancellationToken = default);
    Task<ImportResult<T>> ProcessAsync<T>(Stream stream, ImportFormat format, CancellationToken cancellationToken = default) where T : class;
    Task<ImportResult<T>> ProcessAsync<T>(Stream stream, ImportFormat format, ColumnMappingConfiguration configuration, CancellationToken cancellationToken = default) where T : class;
}
