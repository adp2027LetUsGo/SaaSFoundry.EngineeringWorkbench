import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Add Microsoft.Extensions.DependencyInjection and MappingStatus namespace
content = content.replace('using SaaSFoundry.SDK.Import.AI;', 'using SaaSFoundry.SDK.Import.AI;\nusing SaaSFoundry.SDK.Import.AI.Models;\nusing Microsoft.Extensions.DependencyInjection;')

# 2. Fix System.Text.Encoding namespace collision
content = content.replace('System.Text.Encoding.UTF8.GetBytes', 'global::System.Text.Encoding.UTF8.GetBytes')

# 3. Fix SuggestMappingsAsync parameters (add null for aliases)
content = content.replace('SemanticMapper.SuggestMappingsAsync(schema, allowedFields)', 'SemanticMapper.SuggestMappingsAsync(schema, allowedFields, null)')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
