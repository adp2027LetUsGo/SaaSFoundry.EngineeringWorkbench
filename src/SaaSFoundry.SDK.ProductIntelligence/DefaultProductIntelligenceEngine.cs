using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.SDK.AI;
using SaaSFoundry.SDK.AI.Models;
using SaaSFoundry.SDK.ProductIntelligence.Models;

namespace SaaSFoundry.SDK.ProductIntelligence;

public sealed class DefaultProductIntelligenceEngine : IProductIntelligenceEngine
{
    private readonly IAIEngine _aiEngine;

    public DefaultProductIntelligenceEngine(IAIEngine aiEngine)
    {
        _aiEngine = aiEngine;
    }

    public async Task<ProductIntelligenceReport> AnalyzeAsync(ProductIntelligenceRequest request, CancellationToken cancellationToken = default)
    {
        var report = new ProductIntelligenceReport { Sku = request.Sku };

        // 1. Deterministic SEO Rules (Analysis First)
        AnalyzeTitleSeo(request.Title, report);
        AnalyzeDescriptionSeo(request.Description, report);

        // 2. AI Intelligence Extraction
        var prompt = new AIRequest<string>(
            $@"Analyze the following product data:
Title: {request.Title}
Description: {request.Description}
Tags: {string.Join(", ", request.Tags)}
Attributes: {string.Join(", ", System.Linq.Enumerable.Select(request.Attributes, kv => $"{kv.Key}: {kv.Value}"))}

Extract the following in JSON format conforming exactly to the requested schema:
- extractedFeatures (array of strings): Key selling points or physical features.
- targetAudience (string): Who the product is primarily for.
- tone (string): The brand voice inferred from the description.
- semanticTags (array of strings): Broad concept tags.
- contentGaps (array of strings): What critical information is missing from the description.
- recommendations (array of strings): Actionable ways to improve the product listing.",
            "ProductIntelligenceExtraction",
            TimeSpan.FromSeconds(30)
        );

        var aiResult = await _aiEngine.ExecuteAsync<string, string>(prompt, cancellationToken);
        if (aiResult.Status == AIResultStatus.Success && aiResult.Response != null && !string.IsNullOrWhiteSpace(aiResult.Response.Output))
        {
            try
            {
                var extraction = JsonSerializer.Deserialize(
                    aiResult.Response.Output, 
                    ProductIntelligenceJsonSerializerContext.Default.AIIntelligenceExtractionResult);

                if (extraction != null)
                {
                    report.ExtractedFeatures = extraction.ExtractedFeatures ?? new();
                    report.TargetAudience = extraction.TargetAudience ?? string.Empty;
                    report.Tone = extraction.Tone ?? string.Empty;
                    report.SemanticTags = extraction.SemanticTags ?? new();
                    report.ContentGaps = extraction.ContentGaps ?? new();
                    report.Recommendations = extraction.Recommendations ?? new();
                }
            }
            catch (JsonException)
            {
                // Fallback on serialization error, but do not crash.
            }
        }

        return report;
    }

    private static void AnalyzeTitleSeo(string title, ProductIntelligenceReport report)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            report.SeoFindings.Add(new SeoFinding 
            { 
                Severity = SeoSeverity.Error, 
                Field = "Title", 
                Issue = "Missing Title", 
                Recommendation = "A product title is required for SEO indexing and display.",
                Evidence = "Title field is empty" 
            });
            return;
        }

        if (title.Length < 30)
        {
            report.SeoFindings.Add(new SeoFinding 
            { 
                Severity = SeoSeverity.Warning, 
                Field = "Title", 
                Issue = "Title too short", 
                Recommendation = "Expand title to 50-60 characters incorporating primary keywords.",
                Evidence = $"Length: {title.Length}" 
            });
        }
        else if (title.Length > 70)
        {
            report.SeoFindings.Add(new SeoFinding 
            { 
                Severity = SeoSeverity.Warning, 
                Field = "Title", 
                Issue = "Title too long", 
                Recommendation = "Shorten title to under 70 characters to prevent truncation in search results.",
                Evidence = $"Length: {title.Length}" 
            });
        }

        if (!title.Contains(" ", StringComparison.Ordinal))
        {
            report.SeoFindings.Add(new SeoFinding 
            { 
                Severity = SeoSeverity.Warning, 
                Field = "Title", 
                Issue = "Lack of keywords", 
                Recommendation = "Add descriptive keywords instead of a single word.",
                Evidence = title
            });
        }
    }

    private static void AnalyzeDescriptionSeo(string description, ProductIntelligenceReport report)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            report.SeoFindings.Add(new SeoFinding 
            { 
                Severity = SeoSeverity.Error, 
                Field = "Description", 
                Issue = "Missing Description", 
                Recommendation = "A product description is required to provide details and improve search rankings.",
                Evidence = "Description field is empty" 
            });
            return;
        }

        if (description.Length < 100)
        {
            report.SeoFindings.Add(new SeoFinding 
            { 
                Severity = SeoSeverity.Warning, 
                Field = "Description", 
                Issue = "Description too short", 
                Recommendation = "Provide at least 150-200 words detailing features, benefits, and specifications.",
                Evidence = $"Length: {description.Length}" 
            });
        }

        if (!description.Contains("<") && !description.Contains("\n"))
        {
            report.SeoFindings.Add(new SeoFinding 
            { 
                Severity = SeoSeverity.Warning, 
                Field = "Description", 
                Issue = "Poor readability/structure", 
                Recommendation = "Use paragraphs, bullet points, or HTML tags to break up the text.",
                Evidence = "No structural formatting found" 
            });
        }

        var lowerDesc = description.ToLowerInvariant();
        if (!lowerDesc.Contains("buy") && !lowerDesc.Contains("order") && !lowerDesc.Contains("discover") && !lowerDesc.Contains("shop") && !lowerDesc.Contains("get"))
        {
            report.SeoFindings.Add(new SeoFinding 
            { 
                Severity = SeoSeverity.Info, 
                Field = "Description", 
                Issue = "Missing Call-to-Action", 
                Recommendation = "Include a clear call-to-action (e.g., 'Buy now', 'Order today', 'Discover').",
                Evidence = "No CTA keywords found" 
            });
        }
    }
}
