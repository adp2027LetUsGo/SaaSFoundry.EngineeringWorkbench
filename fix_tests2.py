import os

tests_path = r"C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs"

with open(tests_path, "r", encoding="utf-8") as f:
    code = f.read()

# Fix the injected code: remove SeoAnalyzer
code = code.replace(
    """public ICommerceProductManager CommerceManager;
        public SaaSFoundry.SDK.ProductIntelligence.IProductIntelligenceEngine ProductIntelligence;
        public SaaSFoundry.SDK.ProductIntelligence.ISeoAnalyzer SeoAnalyzer;""",
    """public ICommerceProductManager CommerceManager;
        public SaaSFoundry.SDK.ProductIntelligence.IProductIntelligenceEngine ProductIntelligence;"""
)

code = code.replace(
    """IdempotencyEnforcer = new MockIdempotencyEnforcer();

            var piServices = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            piServices.AddSingleton<SaaSFoundry.SDK.ProductIntelligence.IAIIntelligenceExtractor>(new SaaSFoundry.SDK.ProductIntelligence.DefaultAIIntelligenceExtractor(AIEngine));
            ProductIntelligence = new SaaSFoundry.SDK.ProductIntelligence.ProductIntelligenceEngine(piServices.BuildServiceProvider());
            SeoAnalyzer = new SaaSFoundry.SDK.ProductIntelligence.SeoAnalyzer();
""",
    """IdempotencyEnforcer = new MockIdempotencyEnforcer();

            var piServices = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            piServices.AddSingleton<SaaSFoundry.SDK.ProductIntelligence.IAIIntelligenceExtractor>(new SaaSFoundry.SDK.ProductIntelligence.DefaultAIIntelligenceExtractor(AIEngine));
            ProductIntelligence = new SaaSFoundry.SDK.ProductIntelligence.ProductIntelligenceEngine(piServices.BuildServiceProvider());
"""
)

code = code.replace(
    """                // SEO
                var seoFindings = await SeoAnalyzer.AnalyzeAsync(piReport, default);
                record.SeoFindings = seoFindings;""",
    """                // SEO is part of PI Report
                record.SeoFindings = record.Intelligence?.SeoFindings.ToArray();"""
)

with open(tests_path, "w", encoding="utf-8") as f:
    f.write(code)

print("Tests updated again.")
