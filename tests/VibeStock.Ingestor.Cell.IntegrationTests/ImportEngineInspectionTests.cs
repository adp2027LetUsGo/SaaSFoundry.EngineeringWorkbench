using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SaaSFoundry.Import;
using SaaSFoundry.SDK.Import;
using SaaSFoundry.SDK.Import.Engine;
using SaaSFoundry.SDK.Import.Models;
using VibeStock.Ingestor.Cell.Extensions;

namespace VibeStock.Ingestor.Cell.IntegrationTests;

public class ImportEngineInspectionTests
{
    private readonly IServiceProvider _serviceProvider;

    public ImportEngineInspectionTests()
    {
        var services = new ServiceCollection();
        services.AddSaaSFoundryImport();
        services.AddVibeStockImportDomain();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task InspectAsync_Csv_ReturnsDiscoveredColumns()
    {
        var engine = _serviceProvider.GetRequiredService<IImportEngine>();
        var csvContent = "SKU,Product Name,PVP,Stock,Material\nSKU001,Alpha Vibe,150.00,10,Cotton";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var schema = await engine.InspectAsync(stream, ImportFormat.Csv);

        Assert.NotNull(schema);
        Assert.Equal(5, schema.Columns.Count);
        Assert.Equal("SKU", schema.Columns[0]);
        Assert.Equal("Product Name", schema.Columns[1]);
        Assert.Equal("PVP", schema.Columns[2]);
        Assert.Equal("Stock", schema.Columns[3]);
        Assert.Equal("Material", schema.Columns[4]);
    }
}
