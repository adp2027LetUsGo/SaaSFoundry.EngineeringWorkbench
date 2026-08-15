import os

test_path = r'src\SaaSFoundry.SDK.ProductIntelligence\DefaultProductIntelligenceEngine.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix NativeAOT warning
content = content.replace('JsonSerializer.Serialize(request.Attributes)', 'string.Join(", ", System.Linq.Enumerable.Select(request.Attributes, kv => $"{kv.Key}: {kv.Value}"))')

# Fix IsSuccess
content = content.replace('aiResult.IsSuccess', 'aiResult.Status == AIResultStatus.Success')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
