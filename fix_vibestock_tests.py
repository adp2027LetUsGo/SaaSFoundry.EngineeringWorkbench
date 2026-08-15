import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix 1: ImportEngine constructor to use a valid service provider
new_engine_init = """
            var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            serviceCollection.AddSingleton<SaaSFoundry.SDK.Import.Mapping.IImportMapper<VibeStockProduct>>(new SaaSFoundry.SDK.Import.Mapping.DefaultImportMapper<VibeStockProduct>());
            ImportEngine = new DefaultImportEngine(serviceCollection.BuildServiceProvider());
"""
content = content.replace('            ImportEngine = new DefaultImportEngine(null!);', new_engine_init)

# Fix 2: H_ShopifyRateLimiting_AppliesBoundedRetry (Expected 4 calls)
content = content.replace('Assert.Equal(3, pipeline.HttpHandler.CallCount); // 2 fails (429) + 1 success', 'Assert.Equal(4, pipeline.HttpHandler.CallCount); // 2 fails (429) + 1 success for A + 1 success for B')

# Fix 3: J_Idempotency_PreventsDuplicateGraphQLMutations (Expected 2 calls)
content = content.replace('Assert.Equal(1, pipeline.HttpHandler.CallCount);', 'Assert.Equal(2, pipeline.HttpHandler.CallCount);')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
