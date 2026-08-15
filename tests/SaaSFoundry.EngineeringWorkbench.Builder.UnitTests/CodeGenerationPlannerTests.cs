using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SaaSFoundry.EngineeringWorkbench.Builder.Models;
using SaaSFoundry.EngineeringWorkbench.Builder.Services;

namespace SaaSFoundry.EngineeringWorkbench.Builder.UnitTests;

public class CodeGenerationPlannerTests
{
    [Fact]
    public void Plan_ValidRegistrationMetadata_ProducesDeterministicPlan()
    {
        // Arrange
        var product = new ProductDefinition
        {
            ProductId = "TestProduct",
            Cells = new List<CellDefinition>
            {
                new CellDefinition
                {
                    CellId = "Core.Cell",
                    Capabilities = new List<string> { "persistence", "observability" }
                }
            }
        };

        var plugins = new List<PluginDescriptor>
        {
            new PluginDescriptor
            {
                Id = "plugin1",
                Registrations = new List<CapabilityRegistrationMetadata>
                {
                    new CapabilityRegistrationMetadata { CapabilityId = "persistence", Namespace = "Generated.Persistence", ExtensionMethod = "AddPersistence", RegistrationOrder = 2 },
                    new CapabilityRegistrationMetadata { CapabilityId = "observability", Namespace = "Generated.Observability", ExtensionMethod = "AddObservability", RegistrationOrder = 1 }
                }
            }
        };

        var planner = new CodeGenerationPlanner();

        // Act
        var plan = planner.Plan(product, plugins);

        // Assert
        Assert.NotNull(plan);
        Assert.Single(plan.Cells);
        var cell = plan.Cells.First();
        Assert.Equal(2, cell.Registrations.Count);
        
        // Order deterministically by RegistrationOrder
        Assert.Equal("observability", cell.Registrations[0].CapabilityId);
        Assert.Equal("persistence", cell.Registrations[1].CapabilityId);
    }

    [Fact]
    public void Plan_MissingRegistration_ThrowsInvalidOperationException()
    {
        var product = new ProductDefinition { Cells = new List<CellDefinition> { new CellDefinition { CellId = "C1", Capabilities = new List<string> { "cap1" } } } };
        var plugins = new List<PluginDescriptor>();
        var planner = new CodeGenerationPlanner();

        var ex = Assert.Throws<InvalidOperationException>(() => planner.Plan(product, plugins));
        Assert.Contains("missing registration metadata", ex.Message);
    }
    
    [Fact]
    public void Plan_PopulatesProductDefinition()
    {
        var product = new ProductDefinition 
        { 
            ProductId = "TestProd",
            Communications = new List<CommunicationEdge>
            {
                new CommunicationEdge { Source = "C1", Destination = "C2", Mode = "Bidirectional" }
            }
        };
        var planner = new CodeGenerationPlanner();
        var plugins = new List<PluginDescriptor>();

        var plan = planner.Plan(product, plugins);

        Assert.NotNull(plan.Product);
        Assert.Equal("TestProd", plan.Product.ProductId);
        Assert.Single(plan.Product.Communications);
        Assert.Equal("C1", plan.Product.Communications[0].Source);
    }
}
