using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.SDK.Import;
using SaaSFoundry.SDK.Import.Engine;

namespace SaaSFoundry.Plugins.Import;

public static class ImportServiceCollectionExtensions
{
    public static IServiceCollection AddImportEngine(this IServiceCollection services)
    {
        services.AddSingleton<IImportEngine, DefaultImportEngine>();
        return services;
    }
}
