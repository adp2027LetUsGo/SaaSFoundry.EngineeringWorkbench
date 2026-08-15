using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using SaaSFoundry.EngineeringWorkbench.Builder.Models;
using SaaSFoundry.EngineeringWorkbench.Builder.Services;

namespace SaaSFoundry.EngineeringWorkbench.Builder.UnitTests;

public class HostGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ValidPlan_GeneratesProgramCs()
    {
        var artifactWriter = new ArtifactWriter();
        var generator = new HostGenerator(artifactWriter);

        var plan = new CodeGenerationPlan
        {
            ProductId = "TestProduct",
            Cells = new List<CellGenerationPlan>
            {
                new CellGenerationPlan
                {
                    CellId = "Core.Cell",
                    TargetPath = "Core.Cell",
                    Registrations = new List<CapabilityRegistrationMetadata>
                    {
                        new CapabilityRegistrationMetadata { CapabilityId = "cap1", Namespace = "Ns1", ExtensionMethod = "AddCap1", RegistrationOrder = 1 },
                        new CapabilityRegistrationMetadata { CapabilityId = "cap2", Namespace = "Ns2", ExtensionMethod = "AddCap2", RegistrationOrder = 2 }
                    }
                }
            }
        };

        var targetRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        try
        {
            await generator.GenerateAsync(plan, targetRoot);
            
            var generatedFile = Path.Combine(targetRoot, "Core.Cell", "Generated", "Program.cs");
            Assert.True(File.Exists(generatedFile));
            
            var content = await File.ReadAllTextAsync(generatedFile);
            
            Assert.Contains("using Ns1;", content);
            Assert.Contains("using Ns2;", content);
            Assert.Contains("builder.Services.AddCap1();", content);
            Assert.Contains("builder.Services.AddCap2();", content);
            
            // Validate deterministic order
            var idx1 = content.IndexOf("builder.Services.AddCap1();");
            var idx2 = content.IndexOf("builder.Services.AddCap2();");
            Assert.True(idx1 < idx2);
        }
        finally
        {
            if (Directory.Exists(targetRoot))
                Directory.Delete(targetRoot, true);
        }
    }

    [Fact]
    public async Task GenerateAsync_DuplicateRegistration_ThrowsInvalidOperationException()
    {
        var artifactWriter = new ArtifactWriter();
        var generator = new HostGenerator(artifactWriter);

        var plan = new CodeGenerationPlan
        {
            ProductId = "TestProduct",
            Cells = new List<CellGenerationPlan>
            {
                new CellGenerationPlan
                {
                    CellId = "Core.Cell",
                    TargetPath = "Core.Cell",
                    Registrations = new List<CapabilityRegistrationMetadata>
                    {
                        new CapabilityRegistrationMetadata { CapabilityId = "cap1", Namespace = "Ns1", ExtensionMethod = "AddCap1" },
                        new CapabilityRegistrationMetadata { CapabilityId = "cap1", Namespace = "Ns1", ExtensionMethod = "AddCap1" }
                    }
                }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => generator.GenerateAsync(plan, Path.GetTempPath()));
    }
    
    [Fact]
    public async Task GenerateAsync_MissingNamespace_ThrowsInvalidOperationException()
    {
        var artifactWriter = new ArtifactWriter();
        var generator = new HostGenerator(artifactWriter);

        var plan = new CodeGenerationPlan
        {
            ProductId = "TestProduct",
            Cells = new List<CellGenerationPlan>
            {
                new CellGenerationPlan
                {
                    CellId = "Core.Cell",
                    TargetPath = "Core.Cell",
                    Registrations = new List<CapabilityRegistrationMetadata>
                    {
                        new CapabilityRegistrationMetadata { CapabilityId = "cap1", Namespace = "", ExtensionMethod = "AddCap1" }
                    }
                }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => generator.GenerateAsync(plan, Path.GetTempPath()));
    }
}
