import os

def insert_using(path):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    if 'using SaaSFoundry.SDK.AI.Models;' not in content:
        content = 'using SaaSFoundry.SDK.AI.Models;\n' + content
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)

insert_using(r'tests\SaaSFoundry.SDK.ProductIntelligence.Tests\ProductIntelligenceEngineTests.cs')
insert_using(r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs')

def replace_in_file(path, old, new):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content.replace(old, new))

replace_in_file(r'tests\VibeStock.System.Cell.IntegrationTests\GrpcTransportIntegrationTests.cs', 'InfrastructureService', 'VibeStock.System.Cell.InfrastructureService')
