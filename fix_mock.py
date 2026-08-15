import os

tests_path = r"C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs"

with open(tests_path, "r", encoding="utf-8") as f:
    code = f.read()

mock_snippet = """              if (typeof(TOutput) == typeof(SaaSFoundry.SDK.ProductIntelligence.Models.AIIntelligenceExtractionResult))
              {
                  var extResult = new SaaSFoundry.SDK.ProductIntelligence.Models.AIIntelligenceExtractionResult
                  {
                      TargetAudience = "General",
                      Tone = "Friendly",
                      ContentGaps = new List<string> { "Missing dimensions" }
                  };
                  return Task.FromResult(AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)extResult, AIConfidence.High, "mocked")));
              }
              throw new NotImplementedException();"""

code = code.replace("throw new NotImplementedException();", mock_snippet)

with open(tests_path, "w", encoding="utf-8") as f:
    f.write(code)

print("Mock updated.")
