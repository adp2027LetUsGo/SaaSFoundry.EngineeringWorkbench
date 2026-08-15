using SaaSFoundry.SDK.AI.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SaaSFoundry.SDK.AI;
using SaaSFoundry.SDK.ProductIntelligence;
using SaaSFoundry.SDK.ProductIntelligence.Models;

namespace SaaSFoundry.SDK.ProductIntelligence.Tests;

public class MockAIEngine : IAIEngine
{
    public bool SimulateFailure { get; set; }
    
    public Task<AIResult<TOutput>> ExecuteAsync<TInput, TOutput>(AIRequest<TInput> request, CancellationToken cancellationToken = default)
    {
        if (SimulateFailure) throw new Exception("AI Offline");

        var json = @"{
            ""extractedFeatures"": [""Breathable"", ""Lightweight""],
            ""targetAudience"": ""Casual wearers"",
            ""tone"": ""Relaxed"",
            ""semanticTags"": [""Summer"", ""Comfort""],
            ""contentGaps"": [""Missing washing instructions""],
            ""recommendations"": [""Add material composition""]
        }";
        
        return Task.FromResult(AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)json, AIConfidence.High, "mocked")));
    }
}

public class ProductIntelligenceEngineTests
{
    [Fact]
    public async Task A_AnalyzeAsync_PopulatesIntelligenceFields()
    {
        var engine = new DefaultProductIntelligenceEngine(new MockAIEngine());
        var request = new ProductIntelligenceRequest
        {
            Sku = "SHIRT-1",
            Title = "A perfect summer shirt that is comfortable",
            Description = "Wear this shirt on hot summer days. It is very breathable."
        };

        var report = await engine.AnalyzeAsync(request);

        Assert.Equal("SHIRT-1", report.Sku);
        Assert.Equal(2, report.ExtractedFeatures.Count);
        Assert.Equal("Breathable", report.ExtractedFeatures[0]);
        Assert.Equal("Casual wearers", report.TargetAudience);
        Assert.Equal("Relaxed", report.Tone);
        Assert.Equal(2, report.SemanticTags.Count);
        Assert.Single(report.ContentGaps);
        Assert.Single(report.Recommendations);
    }

    [Fact]
    public async Task B_AnalyzeAsync_EvaluatesSeoRules_MissingTitle()
    {
        var engine = new DefaultProductIntelligenceEngine(new MockAIEngine());
        var request = new ProductIntelligenceRequest
        {
            Sku = "TEST",
            Title = "",
            Description = "Valid description long enough so it doesn't trigger length warnings. Let's make it more than 100 characters. Order now!"
        };

        var report = await engine.AnalyzeAsync(request);

        var titleError = report.SeoFindings.FirstOrDefault(f => f.Field == "Title");
        Assert.NotNull(titleError);
        Assert.Equal(SeoSeverity.Error, titleError.Severity);
        Assert.Equal("Missing Title", titleError.Issue);
    }

    [Fact]
    public async Task C_AnalyzeAsync_EvaluatesSeoRules_DescriptionCta()
    {
        var engine = new DefaultProductIntelligenceEngine(new MockAIEngine());
        var request = new ProductIntelligenceRequest
        {
            Sku = "TEST",
            Title = "A perfectly lengthy title that meets the requirements completely",
            Description = "This description lacks any formatting or calls to action. It is also quite short."
        };

        var report = await engine.AnalyzeAsync(request);

        var ctaWarning = report.SeoFindings.FirstOrDefault(f => f.Issue == "Missing Call-to-Action");
        Assert.NotNull(ctaWarning);
        Assert.Equal(SeoSeverity.Info, ctaWarning.Severity);

        var lengthWarning = report.SeoFindings.FirstOrDefault(f => f.Issue == "Description too short");
        Assert.NotNull(lengthWarning);
        
        var readWarning = report.SeoFindings.FirstOrDefault(f => f.Issue == "Poor readability/structure");
        Assert.NotNull(readWarning);
    }

    [Fact]
    public async Task D_AnalyzeAsync_HandlesAIFailureGracefully()
    {
        var engine = new DefaultProductIntelligenceEngine(new MockAIEngine { SimulateFailure = true });
        var request = new ProductIntelligenceRequest { Sku = "TEST", Title = "Long enough title with keywords", Description = "A very long description that goes over 100 characters so it doesn't fail the SEO check for length. Buy it today to see!" };

        await Assert.ThrowsAsync<Exception>(() => engine.AnalyzeAsync(request));
    }
}
