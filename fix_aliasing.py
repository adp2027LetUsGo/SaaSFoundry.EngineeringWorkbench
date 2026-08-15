import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace Domain and Mapping usings
content = content.replace('using VibeStock.Ingestor.Cell.Domain;', 'using Ingestor::VibeStock.Ingestor.Cell.Domain;')
content = content.replace('using VibeStock.Ingestor.Cell.Mapping;', 'using Ingestor::VibeStock.Ingestor.Cell.Mapping;')

# Prepend extern alias if not already there
if 'extern alias Ingestor;' not in content:
    content = 'extern alias Ingestor;\n' + content

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)

