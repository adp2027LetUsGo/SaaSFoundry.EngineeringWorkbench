import os

for p in ['SaaSFoundry.Plugins.AI', 'SaaSFoundry.Plugins.Import.AI', 'SaaSFoundry.Plugins.Commerce.Shopify', 'SaaSFoundry.Plugins.ProductIntelligence']:
    csproj = os.path.join('src', p, f'{p}.csproj')
    with open(csproj, 'r') as f:
        content = f.read()
    if 'SaaSFoundry.SDK.Core.csproj' not in content:
        content = content.replace('</ItemGroup>', '  <ProjectReference Include="..\\SaaSFoundry.SDK.Core\\SaaSFoundry.SDK.Core.csproj" />\n  </ItemGroup>')
        with open(csproj, 'w') as f:
            f.write(content)

def replace_in_file(path, old, new):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content.replace(old, new))

replace_in_file(r'src\SaaSFoundry.SDK.ProductIntelligence\DefaultProductIntelligenceEngine.cs', 'new AIRequest<string> { Payload = prompt }', 'new AIRequest<string>(prompt, "ProductIntelligence", TimeSpan.FromSeconds(30))')
replace_in_file(r'src\SaaSFoundry.SDK.ProductIntelligence\DefaultProductIntelligenceEngine.cs', 'new AIRequest<string> { Payload = prompt }', 'new AIRequest<string>(prompt, "ProductIntelligence", TimeSpan.FromSeconds(30))')
