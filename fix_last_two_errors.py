import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('pipeline.SemanticMapper.SuggestMappingsAsync(schema, typeof(VibeStockProduct))', 'pipeline.SemanticMapper.SuggestMappingsAsync(schema, new[] { "Name", "Price", "Sku", "Inventory" }, null)')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
