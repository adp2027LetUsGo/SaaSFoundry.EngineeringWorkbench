using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SaaSFoundry.SDK.AI;
using SaaSFoundry.SDK.AI.Models;
using SaaSFoundry.SDK.Import.Models;
using SaaSFoundry.SDK.Import.AI.Engine;
using SaaSFoundry.SDK.Import.AI.Models;
using SaaSFoundry.SDK.Import.AI.Models.AI;

namespace SaaSFoundry.SDK.Import.AI.Tests;

public class MockAIEngine : IAIEngine
{
    public AIResultStatus ResultStatus { get; set; } = AIResultStatus.Success;
    public SemanticMappingProviderResponse? ResponsePayload { get; set; }

    public Task<AIResult<TOutput>> ExecuteAsync<TInput, TOutput>(AIRequest<TInput> request, CancellationToken cancellationToken = default)
    {
        if (ResultStatus != AIResultStatus.Success || ResponsePayload == null)
            return Task.FromResult(AIResult<TOutput>.Failure(ResultStatus, "Mock error"));
            
        var response = new AIResponse<TOutput>((TOutput)(object)ResponsePayload, AIConfidence.Medium, "Tested");
        return Task.FromResult(AIResult<TOutput>.Success(response));
    }
}

public class DefaultAISemanticMapperTests
{
    [Fact]
    public async Task SuggestMappingsAsync_ExactMatch_ReturnsConfirmedBypassingAI()
    {
        var schema = new ImportSchema(new[] { "Sku", "Name" });
        var allowed = new[] { "Sku", "Name", "Price" };

        var engine = new MockAIEngine { ResultStatus = AIResultStatus.Timeout }; // Should not be called
        
        var services = new ServiceCollection();
        services.AddSingleton<IAIEngine>(engine);
        services.AddTransient<IAISemanticMapper, DefaultAISemanticMapper>();
        var sp = services.BuildServiceProvider();

        var mapper = sp.GetRequiredService<IAISemanticMapper>();
        
        var result = await mapper.SuggestMappingsAsync(schema, allowed, null);

        Assert.Equal(2, result.Mappings.Count);
        Assert.All(result.Mappings, m => Assert.Equal(MappingStatus.Confirmed, m.Status));
        Assert.All(result.Mappings, m => Assert.Equal(AIConfidence.High, m.Confidence));
    }

    [Fact]
    public async Task SuggestMappingsAsync_NormalizedMatch_ReturnsConfirmed()
    {
        var schema = new ImportSchema(new[] { "Product Name" });
        var allowed = new[] { "Sku", "ProductName", "Price" };

        var services = new ServiceCollection();
        services.AddTransient<IAISemanticMapper, DefaultAISemanticMapper>();
        var sp = services.BuildServiceProvider();
        var mapper = sp.GetRequiredService<IAISemanticMapper>();
        
        var result = await mapper.SuggestMappingsAsync(schema, allowed, null);

        Assert.Single(result.Mappings);
        Assert.Equal("ProductName", result.Mappings[0].TargetField);
        Assert.Equal(MappingStatus.Confirmed, result.Mappings[0].Status);
    }
    
    [Fact]
    public async Task SuggestMappingsAsync_AliasMatch_ReturnsConfirmed()
    {
        var schema = new ImportSchema(new[] { "PVP" });
        var allowed = new[] { "Sku", "Name", "Price" };
        var aliases = new Dictionary<string, string> { { "PVP", "Price" } };

        var services = new ServiceCollection();
        services.AddTransient<IAISemanticMapper, DefaultAISemanticMapper>();
        var sp = services.BuildServiceProvider();
        var mapper = sp.GetRequiredService<IAISemanticMapper>();
        
        var result = await mapper.SuggestMappingsAsync(schema, allowed, aliases);

        Assert.Single(result.Mappings);
        Assert.Equal("Price", result.Mappings[0].TargetField);
        Assert.Equal(MappingStatus.Confirmed, result.Mappings[0].Status);
    }

    [Fact]
    public async Task SuggestMappingsAsync_Unresolved_UsesAI_ReturnsSuggested()
    {
        var schema = new ImportSchema(new[] { "Ref", "Title" });
        var allowed = new[] { "Sku", "Name", "Price" };

        var aiResponse = new SemanticMappingProviderResponse
        {
            Suggestions = new List<ProviderSuggestedMapping>
            {
                new ProviderSuggestedMapping { SourceColumn = "Ref", TargetField = "Sku", Confidence = "High", Evidence = "Ref implies SKU" },
                new ProviderSuggestedMapping { SourceColumn = "Title", TargetField = "Name", Confidence = "Medium", Evidence = "Title implies product name" }
            }
        };

        var engine = new MockAIEngine { ResponsePayload = aiResponse };
        
        var services = new ServiceCollection();
        services.AddSingleton<IAIEngine>(engine);
        services.AddTransient<IAISemanticMapper, DefaultAISemanticMapper>();
        var sp = services.BuildServiceProvider();
        var mapper = sp.GetRequiredService<IAISemanticMapper>();
        
        var result = await mapper.SuggestMappingsAsync(schema, allowed, null);

        Assert.Equal(2, result.Mappings.Count);
        
        var refMap = result.Mappings.First(m => m.SourceColumn == "Ref");
        Assert.Equal("Sku", refMap.TargetField);
        Assert.Equal(MappingStatus.Suggested, refMap.Status);
        Assert.Equal(AIConfidence.High, refMap.Confidence);
        
        var titleMap = result.Mappings.First(m => m.SourceColumn == "Title");
        Assert.Equal("Name", titleMap.TargetField);
        Assert.Equal(MappingStatus.Suggested, titleMap.Status);
        Assert.Equal(AIConfidence.Medium, titleMap.Confidence);
    }

    [Fact]
    public async Task SuggestMappingsAsync_AIError_ReturnsUnresolved()
    {
        var schema = new ImportSchema(new[] { "Unknown" });
        var allowed = new[] { "Sku" };

        var engine = new MockAIEngine { ResultStatus = AIResultStatus.Timeout };
        
        var services = new ServiceCollection();
        services.AddSingleton<IAIEngine>(engine);
        services.AddTransient<IAISemanticMapper, DefaultAISemanticMapper>();
        var sp = services.BuildServiceProvider();
        var mapper = sp.GetRequiredService<IAISemanticMapper>();
        
        var result = await mapper.SuggestMappingsAsync(schema, allowed, null);

        Assert.Single(result.Mappings);
        Assert.Equal(MappingStatus.Unresolved, result.Mappings[0].Status);
        Assert.Null(result.Mappings[0].TargetField);
    }
}
