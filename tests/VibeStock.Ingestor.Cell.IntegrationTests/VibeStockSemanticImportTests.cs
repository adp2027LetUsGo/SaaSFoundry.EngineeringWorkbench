using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SaaSFoundry.Import;
using SaaSFoundry.SDK.Import;
using SaaSFoundry.SDK.Import.AI;
using SaaSFoundry.SDK.Import.AI.Models;
using SaaSFoundry.SDK.Import.Mapping;
using SaaSFoundry.SDK.Import.Models;
using VibeStock.Ingestor.Cell.Domain;
using VibeStock.Ingestor.Cell.Extensions;

namespace VibeStock.Ingestor.Cell.IntegrationTests;

// Mock IAIEngine because the standard integration tests don't have a real AI Provider configured.
public class MockIntegrationAIEngine : SaaSFoundry.SDK.AI.IAIEngine
{
    public Task<SaaSFoundry.SDK.AI.Models.AIResult<TOutput>> ExecuteAsync<TInput, TOutput>(SaaSFoundry.SDK.AI.Models.AIRequest<TInput> request, CancellationToken cancellationToken = default)
    {
        var aiResponse = new SaaSFoundry.SDK.Import.AI.Models.AI.SemanticMappingProviderResponse
        {
            Suggestions = new System.Collections.Generic.List<SaaSFoundry.SDK.Import.AI.Models.AI.ProviderSuggestedMapping>
            {
                new SaaSFoundry.SDK.Import.AI.Models.AI.ProviderSuggestedMapping { SourceColumn = "Referencia", TargetField = "Sku", Confidence = "High", Evidence = "" },
                new SaaSFoundry.SDK.Import.AI.Models.AI.ProviderSuggestedMapping { SourceColumn = "Titulo", TargetField = "Name", Confidence = "High", Evidence = "" },
                new SaaSFoundry.SDK.Import.AI.Models.AI.ProviderSuggestedMapping { SourceColumn = "Costo", TargetField = "Price", Confidence = "High", Evidence = "" },
                new SaaSFoundry.SDK.Import.AI.Models.AI.ProviderSuggestedMapping { SourceColumn = "Cantidad", TargetField = "Inventory", Confidence = "High", Evidence = "" }
            }
        };

        var response = new SaaSFoundry.SDK.AI.Models.AIResponse<TOutput>((TOutput)(object)aiResponse, SaaSFoundry.SDK.AI.Models.AIConfidence.High, "Tested");
        return Task.FromResult(SaaSFoundry.SDK.AI.Models.AIResult<TOutput>.Success(response));
    }
}

public class VibeStockSemanticImportTests
{
    private readonly IServiceProvider _serviceProvider;

    public VibeStockSemanticImportTests()
    {
        var services = new ServiceCollection();
        services.AddSaaSFoundryImport();
        services.AddVibeStockImportDomain();
        // Add Import AI
        services.AddSingleton<IAISemanticMapper, SaaSFoundry.SDK.Import.AI.Engine.DefaultAISemanticMapper>();
        services.AddSingleton<SaaSFoundry.SDK.AI.IAIEngine, MockIntegrationAIEngine>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task EndToEnd_SemanticMapping_HumanApproval_Process()
    {
        var engine = _serviceProvider.GetRequiredService<IImportEngine>();
        var semanticMapper = _serviceProvider.GetRequiredService<IAISemanticMapper>();
        var configurableMapper = _serviceProvider.GetRequiredService<IImportMapper<VibeStockProduct>>() as IConfigurableImportMapper<VibeStockProduct>;
        
        Assert.NotNull(configurableMapper);

        // 1. Inspect
        var csvContent = "Referencia,Titulo,Costo,Cantidad\nSKU-099,Omega Shirt,59.99,100";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        
        var schema = await engine.InspectAsync(stream, ImportFormat.Csv);
        
        // 2. Semantic Map
        var semanticResult = await semanticMapper.SuggestMappingsAsync(schema, configurableMapper.SupportedTargetFields, null);
        
        // 3. Verify AI suggested everything
        Assert.Equal(4, semanticResult.Mappings.Count);
        Assert.All(semanticResult.Mappings, m => Assert.Equal(MappingStatus.Suggested, m.Status));
        
        // 4. Simulate human approval (changing status to Confirmed)
        foreach (var mapping in semanticResult.Mappings)
        {
            mapping.Status = MappingStatus.Confirmed;
        }
        
        // 5. ToConfiguration
        var config = semanticResult.ToConfiguration();
        
        // 6. Process with configuration
        stream.Position = 0; // reset stream
        var finalResult = await engine.ProcessAsync<VibeStockProduct>(stream, ImportFormat.Csv, config);
        
        // Assert
        Assert.Single(finalResult.Rows);
        var row = finalResult.Rows[0];
        Assert.Equal(SaaSFoundry.SDK.Import.Models.ImportCategory.Valid, row.Category);
        Assert.Equal("SKU-099", row.Data.Sku);
        Assert.Equal("Omega Shirt", row.Data.Name);
        Assert.Equal(59.99m, row.Data.Price);
        Assert.Equal(100, row.Data.Inventory);
    }
}
