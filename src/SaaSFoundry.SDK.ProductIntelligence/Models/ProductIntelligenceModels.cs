using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SaaSFoundry.SDK.ProductIntelligence.Models;

public enum ConceptSource
{
    Factual,
    Inferred
}

public enum IntelligenceStatus
{
    Suggested,
    Confirmed,
    Rejected,
    Unresolved
}

public enum SeoSeverity
{
    Info,
    Warning,
    Error
}

public sealed class SeoFinding
{
    public SeoSeverity Severity { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string? Evidence { get; set; }
}

public sealed class ProductIntelligenceRequest
{
    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<string> Tags { get; set; } = new List<string>();
    public IReadOnlyDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
}

public sealed class ProductIntelligenceReport
{
    public string Sku { get; set; } = string.Empty;
    public List<string> ExtractedFeatures { get; set; } = new();
    public string TargetAudience { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public List<string> SemanticTags { get; set; } = new();
    public List<string> ContentGaps { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<SeoFinding> SeoFindings { get; set; } = new();
}

public sealed class AIIntelligenceExtractionResult
{
    [JsonPropertyName("extractedFeatures")]
    public List<string> ExtractedFeatures { get; set; } = new();

    [JsonPropertyName("targetAudience")]
    public string TargetAudience { get; set; } = string.Empty;

    [JsonPropertyName("tone")]
    public string Tone { get; set; } = string.Empty;

    [JsonPropertyName("semanticTags")]
    public List<string> SemanticTags { get; set; } = new();

    [JsonPropertyName("contentGaps")]
    public List<string> ContentGaps { get; set; } = new();

    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; } = new();
}

[JsonSerializable(typeof(AIIntelligenceExtractionResult))]
public partial class ProductIntelligenceJsonSerializerContext : JsonSerializerContext { }
