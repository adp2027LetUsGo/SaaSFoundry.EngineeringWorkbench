import os

tests_path = r"C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs"

with open(tests_path, "r", encoding="utf-8") as f:
    code = f.read()

# We need to inject PI and SEO into the pipeline.
# Find EndToEndPipeline and add them.
code = code.replace(
    "public ICommerceProductManager CommerceManager;",
    """public ICommerceProductManager CommerceManager;
        public SaaSFoundry.SDK.ProductIntelligence.IProductIntelligenceEngine ProductIntelligence;
        public SaaSFoundry.SDK.ProductIntelligence.ISeoAnalyzer SeoAnalyzer;"""
)

# And initialize them:
init_code = """
            var piServices = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            piServices.AddSingleton<SaaSFoundry.SDK.ProductIntelligence.IAIIntelligenceExtractor>(new SaaSFoundry.SDK.ProductIntelligence.DefaultAIIntelligenceExtractor(AIEngine));
            ProductIntelligence = new SaaSFoundry.SDK.ProductIntelligence.ProductIntelligenceEngine(piServices.BuildServiceProvider());
            SeoAnalyzer = new SaaSFoundry.SDK.ProductIntelligence.SeoAnalyzer();
"""
code = code.replace(
    "IdempotencyEnforcer = new MockIdempotencyEnforcer();",
    "IdempotencyEnforcer = new MockIdempotencyEnforcer();\n" + init_code
)

# Now, update SyncToShopifyAsync to use them
sync_code = """
                // Product Intelligence
                var piRequest = new SaaSFoundry.SDK.ProductIntelligence.Models.ProductIntelligenceRequest
                {
                    Sku = record.Sku,
                    Title = record.Name,
                    Description = record.Description,
                    Tags = record.Tags
                };
                var piReport = await ProductIntelligence.AnalyzeAsync(piRequest, default);
                record.Intelligence = piReport;
                
                // SEO
                var seoFindings = await SeoAnalyzer.AnalyzeAsync(piReport, default);
                record.SeoFindings = seoFindings;

                // Domain Map: VibeStock -> Commerce
                var cProduct = new CommerceProduct 
                {
                    Title = record.Name,
                    Description = record.Intelligence.ContentGaps.Count == 0 ? record.Description : record.Description + " [Enhanced]",
                    Vendor = "VibeStock",
                    Variants = new List<CommerceVariant> 
                    { 
                        new CommerceVariant { Sku = record.Sku, Price = record.Price, InventoryQuantity = record.Inventory } 
                    }
                };"""
                
code = code.replace(
    """                // Domain Map: VibeStock -> Commerce
                var cProduct = new CommerceProduct 
                {
                    Title = record.Name,
                    Vendor = "VibeStock",
                    Variants = new List<CommerceVariant> 
                    { 
                        new CommerceVariant { Sku = record.Sku, Price = record.Price, InventoryQuantity = record.Inventory } 
                    }
                };""",
    sync_code
)

with open(tests_path, "w", encoding="utf-8") as f:
    f.write(code)

print("Tests updated.")
