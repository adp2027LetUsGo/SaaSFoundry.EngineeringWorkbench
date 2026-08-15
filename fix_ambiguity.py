import os

csproj_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStock.System.Cell.IntegrationTests.csproj'
with open(csproj_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Add Ingestor reference with Aliases attribute
reference = '<ProjectReference Include="..\\..\\src\\VibeStock.Ingestor.Cell\\VibeStock.Ingestor.Cell.csproj" Aliases="Ingestor" />'
content = content.replace('</ItemGroup>', f'  {reference}\n  </ItemGroup>', 1)

with open(csproj_path, 'w', encoding='utf-8') as f:
    f.write(content)

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the standard using with the aliased using
content = content.replace('using VibeStock.Ingestor.Cell.Models;', 'extern alias Ingestor;\nusing Ingestor::VibeStock.Ingestor.Cell.Models;')
content = content.replace('using VibeStock.Ingestor.Cell;', '')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)

