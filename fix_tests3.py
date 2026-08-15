import os

tests_path = r"C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs"

with open(tests_path, "r", encoding="utf-8") as f:
    code = f.read()

code = code.replace(
    """var piServices = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            piServices.AddSingleton<SaaSFoundry.SDK.ProductIntelligence.IAIIntelligenceExtractor>(new SaaSFoundry.SDK.ProductIntelligence.DefaultAIIntelligenceExtractor(AIEngine));
            ProductIntelligence = new SaaSFoundry.SDK.ProductIntelligence.ProductIntelligenceEngine(piServices.BuildServiceProvider());""",
    """ProductIntelligence = new SaaSFoundry.SDK.ProductIntelligence.DefaultProductIntelligenceEngine(AIEngine);"""
)

with open(tests_path, "w", encoding="utf-8") as f:
    f.write(code)

print("Tests updated again and again.")
