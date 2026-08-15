import os

projects_to_fix = [
    r'src\SaaSFoundry.EngineeringWorkbench.Application\SaaSFoundry.EngineeringWorkbench.Application.csproj',
    r'src\SaaSFoundry.EngineeringWorkbench.UI\SaaSFoundry.EngineeringWorkbench.UI.csproj',
    r'tests\SaaSFoundry.Plugins.Observability.UnitTests\SaaSFoundry.Plugins.Observability.UnitTests.csproj'
]

for p in projects_to_fix:
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()
    
    if '<NoWarn>' not in content:
        content = content.replace('</PropertyGroup>', '  <NoWarn>$(NoWarn);IL2026;IL3050;IL2104</NoWarn>\n  </PropertyGroup>')
        
    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)
