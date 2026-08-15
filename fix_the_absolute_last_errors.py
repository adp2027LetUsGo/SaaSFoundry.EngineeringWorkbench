import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. TargetProperty -> TargetField
content = content.replace('TargetProperty ==', 'TargetField ==')

# 2. ValidRecords
content = content.replace('result.ValidRecords)', 'result.Rows.Where(r => r.Category == SaaSFoundry.SDK.Import.Models.ImportCategory.Valid).Select(r => r.Data).ToList())')
content = content.replace('result.ValidRecords[0]', 'result.Rows.Where(r => r.Category == SaaSFoundry.SDK.Import.Models.ImportCategory.Valid).Select(r => r.Data).ToList()[0]')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
