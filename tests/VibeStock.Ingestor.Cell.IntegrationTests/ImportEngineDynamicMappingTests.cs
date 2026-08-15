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
using VibeStock.Ingestor.Cell.Domain;
using VibeStock.Ingestor.Cell.Extensions;

namespace VibeStock.Ingestor.Cell.IntegrationTests;

public class ImportEngineDynamicMappingTests
{
    private readonly IServiceProvider _serviceProvider;

    public ImportEngineDynamicMappingTests()
    {
        var services = new ServiceCollection();
        services.AddSaaSFoundryImport();
        services.AddVibeStockImportDomain();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void ColumnMappingConfiguration_InvalidTargets_Throws()
    {
        var mappings = new[] { new ColumnMapping("PVP", "invalid_price") };
        var config = new ColumnMappingConfiguration(mappings);
        var allowed = new[] { "sku", "name", "price", "inventory" };

        var ex = Assert.Throws<InvalidOperationException>(() => config.Validate(allowed));
        Assert.Contains("Unknown target fields", ex.Message);
    }

    [Fact]
    public void ColumnMappingConfiguration_DuplicateTargets_Throws()
    {
        var mappings = new[] { 
            new ColumnMapping("PVP1", "price"),
            new ColumnMapping("PVP2", "price")
        };
        var config = new ColumnMappingConfiguration(mappings);
        var allowed = new[] { "sku", "name", "price", "inventory" };

        var ex = Assert.Throws<InvalidOperationException>(() => config.Validate(allowed));
        Assert.Contains("Duplicate target fields", ex.Message);
    }

    [Fact]
    public async Task ProcessAsync_WithDynamicMapping_SuccessfullyTranslatesColumns()
    {
        var engine = _serviceProvider.GetRequiredService<IImportEngine>();
        
        // CSV with completely different column names
        var csvContent = "Ref,Title,Cost,Qty\nSKU001,Alpha Vibe,150.00,10";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Create mapping configuration
        var mappings = new[] {
            new ColumnMapping("Ref", "sku"),
            new ColumnMapping("Title", "name"),
            new ColumnMapping("Cost", "price"),
            new ColumnMapping("Qty", "inventory")
        };
        var config = new ColumnMappingConfiguration(mappings);

        // Process
        var result = await engine.ProcessAsync<VibeStockProduct>(stream, ImportFormat.Csv, config);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Rows);
        
        var row = result.Rows[0];
        Assert.Equal(ImportCategory.Valid, row.Category);
        Assert.Equal("SKU001", row.Data.Sku);
        Assert.Equal("Alpha Vibe", row.Data.Name);
        Assert.Equal(150m, row.Data.Price);
        Assert.Equal(10, row.Data.Inventory);
    }
}
