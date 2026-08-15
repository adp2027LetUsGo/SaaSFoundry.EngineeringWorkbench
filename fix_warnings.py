import os
import glob

# 1. Update OpenTelemetry in csproj and python generators
paths_to_check = []
for root, _, files in os.walk(r'c:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\src'):
    for file in files:
        if file.endswith('.csproj'):
            paths_to_check.append(os.path.join(root, file))

for root, _, files in os.walk(r'C:\Users\armando\.gemini\antigravity\brain\5c7726f6-290a-42e8-bba5-a32e6505f83c\scratch'):
    for file in files:
        if file.endswith('.py'):
            paths_to_check.append(os.path.join(root, file))

for p in paths_to_check:
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()
    
    new_content = content.replace('"1.11.1"', '"1.11.2"')
    
    # Remove duplicate Grpc.AspNetCore 2.62.0 and Grpc.Tools 2.62.0
    new_content = new_content.replace('<PackageReference Include="Grpc.AspNetCore" Version="2.62.0" />', '')
    new_content = new_content.replace('<PackageReference Include="Grpc.Tools" Version="2.62.0">', '<PackageReference Include="Grpc.Tools" Version="2.62.0" REMOVED="true">') # Some might have children
    new_content = new_content.replace('<PackageReference Include="Grpc.Tools" Version="2.62.0" />', '')

    if new_content != content:
        with open(p, 'w', encoding='utf-8') as f:
            f.write(new_content)
