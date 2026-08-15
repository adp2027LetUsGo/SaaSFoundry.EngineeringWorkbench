using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.SDK.Import.Engine;
using SaaSFoundry.SDK.Import.Mapping;
using SaaSFoundry.SDK.Import.DataQuality;
using VibeStock.Ingestor.Cell.Domain;
using VibeStock.Ingestor.Cell.Mapping;
using VibeStock.Ingestor.Cell.DataQuality;

namespace VibeStock.Ingestor.Cell.Extensions;

public static class IngestorImportSetup
{
    public static IServiceCollection AddVibeStockImportDomain(this IServiceCollection services)
    {
        services.AddSingleton<IImportMapper<VibeStockProduct>, ProductImportMapper>();
        services.AddSingleton<IDataQualityRule<VibeStockProduct>, ProductDataQualityRule>();
        return services;
    }
}
