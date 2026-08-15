import os

def replace_in_file(path, old, new):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content.replace(old, new))

# Fix DefaultProductIntelligenceEngine
p1 = r'src\SaaSFoundry.SDK.ProductIntelligence\DefaultProductIntelligenceEngine.cs'
replace_in_file(p1, 'result.IsSuccess', 'result.Status == AIResultStatus.Success')
replace_in_file(p1, 'result.Data', 'result.Response?.Output')

# Fix Tests
p2 = r'tests\SaaSFoundry.SDK.ProductIntelligence.Tests\ProductIntelligenceEngineTests.cs'
replace_in_file(p2, 'AIResult<TOutput>.Success((TOutput)(object)mockResult)', 'AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)mockResult, AIConfidence.High, "mocked"))')
replace_in_file(p2, 'AIResult<TOutput>.Failure("AI Error")', 'AIResult<TOutput>.Failure(AIResultStatus.TransientFailure, "AI Error")')

p3 = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
replace_in_file(p3, 'AIResult<TOutput>.Success((TOutput)(object)mockResult)', 'AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)mockResult, AIConfidence.High, "mocked"))')
replace_in_file(p3, 'AIResult<TOutput>.Success((TOutput)(object)mockConceptResult)', 'AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)mockConceptResult, AIConfidence.High, "mocked"))')
replace_in_file(p3, 'AIResult<TOutput>.Failure("Simulated AI Failure")', 'AIResult<TOutput>.Failure(AIResultStatus.TransientFailure, "Simulated AI Failure")')

# Add missing ProjectReference for Plugins.Authoring
for p in ['SaaSFoundry.Plugins.AI', 'SaaSFoundry.Plugins.Import.AI', 'SaaSFoundry.Plugins.Commerce.Shopify', 'SaaSFoundry.Plugins.ProductIntelligence']:
    csproj = os.path.join('src', p, f'{p}.csproj')
    with open(csproj, 'r') as f:
        content = f.read()
    if 'SaaSFoundry.SDK.Plugins.csproj' not in content:
        content = content.replace('</ItemGroup>', '  <ProjectReference Include="..\\SaaSFoundry.SDK.Plugins\\SaaSFoundry.SDK.Plugins.csproj" />\n  </ItemGroup>')
        with open(csproj, 'w') as f:
            f.write(content)

