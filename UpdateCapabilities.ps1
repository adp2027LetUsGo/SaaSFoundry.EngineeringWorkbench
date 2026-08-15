$files = Get-ChildItem -Path "c:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\src\SaaSFoundry.Plugins*" -Recurse -Filter "*Capability.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    
    $replacement = @"
    public Task GenerateArtifactsAsync(IPluginExecutionContext context, CancellationToken cancellationToken)
    {
        var stagingOpt = System.Linq.Enumerable.FirstOrDefault(context.Arguments, a => a.StartsWith("--extraction-path="));
        if (stagingOpt != null)
        {
            var path = stagingOpt.Substring("--extraction-path=".Length);
            var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator(Id, "1.0.0", "1.0.0");
            var result = generator.Generate(_descriptors);
            var json = System.Text.Json.JsonSerializer.Serialize(result.GeneratedArtifacts);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, json);
        }
        return Task.CompletedTask;
    }
"@
    
    $content = $content -replace '(?s)public Task GenerateArtifactsAsync\(IPluginExecutionContext context, CancellationToken cancellationToken\) => Task\.CompletedTask;', $replacement
    Set-Content -Path $file.FullName -Value $content -NoNewline
}
