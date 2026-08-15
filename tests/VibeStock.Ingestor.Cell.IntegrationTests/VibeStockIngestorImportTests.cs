using System;
using System.IO;
using System.Linq;
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

public class VibeStockIngestorImportTests
{
    private readonly IServiceProvider _serviceProvider;

    public VibeStockIngestorImportTests()
    {
        var services = new ServiceCollection();
        
        // 1. Generic Engine Bootstrapping (From Factory Generation)
        services.AddSaaSFoundryImport();
        
        // 2. VibeStock Domain Bootstrapping
        services.AddVibeStockImportDomain();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ParseCsv_ToVibeStockProduct_ValidatesDomainRules()
    {
        // Arrange
        var engine = _serviceProvider.GetRequiredService<IImportEngine>();
        var csvContent = "SKU,Name,Price,Inventory\nSKU001,Alpha Vibe,150.00,10\nSKU002,,100.00,-5\n,Ghost Vibe,-10.00,0\nSKU004,Beta Vibe,20.00,0";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await engine.ProcessAsync<VibeStockProduct>(stream, ImportFormat.Csv);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Rows.Count);

        // Row 1: Valid
        var row1 = result.Rows[0];
        Assert.Equal("SKU001", row1.Data.Sku);
        Assert.Empty(row1.Diagnostics);
        Assert.Equal(ImportCategory.Valid, row1.Category);

        // Row 2: Invalid (No name, negative inventory)
        var row2 = result.Rows[1];
        Assert.Equal(ImportCategory.Invalid, row2.Category);
        Assert.Contains(row2.Diagnostics, d => d.Message.Contains("Name is required"));
        Assert.Contains(row2.Diagnostics, d => d.Message.Contains("Inventory cannot be negative"));

        // Row 3: Invalid (No SKU, negative price, zero inventory warning)
        var row3 = result.Rows[2];
        Assert.Equal(ImportCategory.Invalid, row3.Category);
        Assert.Contains(row3.Diagnostics, d => d.Message.Contains("SKU is required"));
        Assert.Contains(row3.Diagnostics, d => d.Message.Contains("Price must be greater than 0"));
        Assert.Contains(row3.Diagnostics, d => d.Category == ImportCategory.Warning && d.Message.Contains("exactly 0"));

        // Row 4: Valid with Warning (Zero inventory)
        var row4 = result.Rows[3];
        Assert.Equal(ImportCategory.Warning, row4.Category); // Warnings do not invalidate the record, but mark as Warning
        Assert.Single(row4.Diagnostics);
        Assert.Equal(ImportCategory.Warning, row4.Diagnostics.First().Category);
    }
}
