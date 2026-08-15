import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix MapColumnsAsync -> SuggestMappingsAsync
content = content.replace('MapColumnsAsync', 'SuggestMappingsAsync')

# Fix DefaultAISemanticMapper constructor
content = content.replace('new DefaultAISemanticMapper(AIEngine)', 'new DefaultAISemanticMapper(new Microsoft.Extensions.DependencyInjection.ServiceCollection().AddSingleton<SaaSFoundry.SDK.AI.IAIEngine>(AIEngine).BuildServiceProvider())')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
