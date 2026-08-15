import os

tests_path = r"C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs"

with open(tests_path, "r", encoding="utf-8") as f:
    code = f.read()

mock_snippet = """              if (typeof(TOutput) == typeof(string))
              {
                  var json = @"{
                      ""ExtractedFeatures"": [""mock""],
                      ""TargetAudience"": ""General"",
                      ""Tone"": ""Friendly"",
                      ""SemanticTags"": [""mock""],
                      ""ContentGaps"": [""Missing dimensions""],
                      ""Recommendations"": [""mock""]
                  }";
                  return Task.FromResult(AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)json, AIConfidence.High, "mocked")));
              }
              throw new NotImplementedException();"""

code = code.replace("""              if (typeof(TOutput) == typeof(SaaSFoundry.SDK.ProductIntelligence.Models.AIIntelligenceExtractionResult))
              {
                  var extResult = new SaaSFoundry.SDK.ProductIntelligence.Models.AIIntelligenceExtractionResult
                  {
                      TargetAudience = "General",
                      Tone = "Friendly",
                      ContentGaps = new List<string> { "Missing dimensions" }
                  };
                  return Task.FromResult(AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)extResult, AIConfidence.High, "mocked")));
              }
              throw new NotImplementedException();""", mock_snippet)

with open(tests_path, "w", encoding="utf-8") as f:
    f.write(code)

print("Mock string updated.")
