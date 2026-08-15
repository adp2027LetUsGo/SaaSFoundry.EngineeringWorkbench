using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.SDK.Import.DataQuality;
using SaaSFoundry.SDK.Import.Engine;
using SaaSFoundry.SDK.Import.Mapping;
using SaaSFoundry.SDK.Import.Models;
using Xunit;

namespace SaaSFoundry.SDK.Import.UnitTests;

public class TestProduct
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class TestProductMapper : IImportMapper<TestProduct>
{
    public TestProduct Map(IReadOnlyDictionary<string, string> row)
    {
        return new TestProduct
        {
            Id = row.GetValueOrDefault("Id", string.Empty),
            Name = row.GetValueOrDefault("Name", string.Empty),
            Price = decimal.TryParse(row.GetValueOrDefault("Price", "0"), out var p) ? p : 0
        };
    }
}

public class TestPriceRule : IDataQualityRule<TestProduct>
{
    public ValueTask EvaluateAsync(ImportRecord<TestProduct> record)
    {
        if (record.Data != null && record.Data.Price <= 0)
        {
            record.Diagnostics.Add(new ImportDiagnostic
            {
                Category = ImportCategory.Invalid,
                Message = "Price must be greater than zero.",
                Field = "Price"
            });
        }
        return ValueTask.CompletedTask;
    }
}

public class DefaultImportEngineTests
{
    [Fact]
    public async Task ProcessAsync_AppliesMappingAndRules()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IImportMapper<TestProduct>, TestProductMapper>();
        services.AddSingleton<IDataQualityRule<TestProduct>, TestPriceRule>();
        var provider = services.BuildServiceProvider();
        
        var engine = new DefaultImportEngine(provider);
        var csv = "Id,Name,Price\n1,Good Product,10.0\n2,Free Product,0.0";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        
        var result = await engine.ProcessAsync<TestProduct>(stream, ImportFormat.Csv, CancellationToken.None);
        
        Assert.Equal(2, result.TotalRows);
        Assert.Equal(1, result.ValidRows);
        Assert.Equal(1, result.InvalidRows);
        
        Assert.Equal(ImportCategory.Valid, result.Rows[0].Category);
        Assert.Equal(10.0m, result.Rows[0].Data!.Price);
        
        Assert.Equal(ImportCategory.Invalid, result.Rows[1].Category);
        Assert.Single(result.Rows[1].Diagnostics);
    }
}
