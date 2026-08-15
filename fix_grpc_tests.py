import os

path = r'tests\VibeStock.System.Cell.IntegrationTests\GrpcTransportIntegrationTests.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('global::VibeStock.System.Cell.InfrastructureService.InfrastructureServiceClient', 'SaaSFoundry.Transport.Generated.InfrastructureService.InfrastructureServiceClient')
content = content.replace('VibeStock.System.Cell.InfrastructureService.VibeStock.System.Cell.InfrastructureServiceClient', 'SaaSFoundry.Transport.Generated.InfrastructureService.InfrastructureServiceClient')

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)
