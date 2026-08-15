import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Fix System.Text namespace collision
content = content.replace('System.Text.Json.JsonSerializer', 'global::System.Text.Json.JsonSerializer')

# 2. Fix MapSchemaAsync -> MapColumnsAsync
content = content.replace('MapSchemaAsync', 'MapColumnsAsync')

# 3. Add MappingStatus using
content = content.replace('using SaaSFoundry.SDK.Import;', 'using SaaSFoundry.SDK.Import;\nusing SaaSFoundry.SDK.Import.Mapping;')

# 4. Fix AIResultStatus
content = content.replace('AIResultStatus.TransientFailure', 'AIResultStatus.Unavailable')

# 5. Fix Confidence double to string
content = content.replace('Confidence = 0.95', 'Confidence = "High"')
content = content.replace('Confidence = 0.90', 'Confidence = "High"')
content = content.replace('Confidence = 0.85', 'Confidence = "High"')

# 6. Fix DefaultAISemanticMapper constructor
content = content.replace('new DefaultAISemanticMapper(AIEngine, null!)', 'new DefaultAISemanticMapper(AIEngine)')

# 7. Fix Title to Name in VibeStockProduct
content = content.replace('Title = "A"', 'Name = "A"')
content = content.replace('Title = "B"', 'Name = "B"')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
