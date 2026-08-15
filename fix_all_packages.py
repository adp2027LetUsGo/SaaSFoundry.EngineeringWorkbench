import os
import re

paths_to_check = []
for root, _, files in os.walk(r'c:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\src'):
    for file in files:
        if file.endswith('.csproj'):
            paths_to_check.append(os.path.join(root, file))

for root, _, files in os.walk(r'c:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\tests'):
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
    
    new_content = content
    # Upgrade ANY OpenTelemetry to 1.12.0
    new_content = re.sub(r'OpenTelemetry(\.[^"]+)?" Version="1\.11\.2"', r'OpenTelemetry\1" Version="1.12.0"', new_content)
    # Also handle the 1.11.x ones just in case
    new_content = re.sub(r'OpenTelemetry(\.[^"]+)?" Version="1\.11\.[0-9]+"', r'OpenTelemetry\1" Version="1.12.0"', new_content)

    if new_content != content:
        with open(p, 'w', encoding='utf-8') as f:
            f.write(new_content)
