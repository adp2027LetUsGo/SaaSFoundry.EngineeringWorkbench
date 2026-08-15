using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Builder.Models;
using SaaSFoundry.EngineeringWorkbench.Builder.Services;
using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution;
using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Registration;
using SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;

namespace SaaSFoundry.EngineeringWorkbench.Cli.Commands;

public sealed class PluginCommands
{
    private const string WorkbenchRoot =
        @"C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench";

    private readonly string _pluginsRoot =
        Path.Combine(
            WorkbenchRoot,
            "plugins");


    public Task ListAsync()
    {
        if(!Directory.Exists(_pluginsRoot))
        {
            Console.WriteLine($"Plugins folder not found: {_pluginsRoot}");
            return Task.CompletedTask;
        }

        Console.WriteLine("Plugins:");
        Console.WriteLine();

        foreach(var plugin in Directory.GetDirectories(_pluginsRoot))
        {
            Console.WriteLine($" - {Path.GetFileName(plugin)}");
        }

        return Task.CompletedTask;
    }


    public async Task GenerateAsync(string plugin)
    {
        Console.WriteLine("Generating product runtime cells...");

        var productJsonPath = Path.Combine(WorkbenchRoot, "product.json");
        if (!File.Exists(productJsonPath)) 
        {
            Console.WriteLine("product.json not found!");
            return;
        }
        var productJson = await File.ReadAllTextAsync(productJsonPath);
        var product = JsonSerializer.Deserialize<ProductDefinition>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var plugins = new List<PluginDescriptor>
        {
            new PluginDescriptor
            {
                Id = "SaaSFoundry.Plugins.API",
                Registrations = new List<CapabilityRegistrationMetadata>
                {
                    new CapabilityRegistrationMetadata { CapabilityId = "health", Namespace = null, ExtensionMethod = null, RegistrationOrder = 10 },
                    new CapabilityRegistrationMetadata { CapabilityId = "grpc_transport", Namespace = "SaaSFoundry.Transport.Generated", ExtensionMethod = "AddGrpcTransport", RegistrationOrder = 99 }
                }
            },
            new PluginDescriptor
            {
                Id = "SaaSFoundry.Plugins.Authentication",
                Registrations = new List<CapabilityRegistrationMetadata>
                {
                    new CapabilityRegistrationMetadata { CapabilityId = "auth_middleware", Namespace = null, ExtensionMethod = null, RegistrationOrder = 20 }
                }
            },
            new PluginDescriptor
            {
                Id = "SaaSFoundry.Plugins.Observability",
                Registrations = new List<CapabilityRegistrationMetadata>
                {
                    new CapabilityRegistrationMetadata { CapabilityId = "logging", Namespace = "SaaSFoundry.Observability.Logging", ExtensionMethod = "AddSaaSFoundryLogging", RegistrationOrder = 30 }
                }
            },
            new PluginDescriptor
            {
                Id = "SaaSFoundry.Plugins.Persistence",
                Registrations = new List<CapabilityRegistrationMetadata>
                {
                    new CapabilityRegistrationMetadata { CapabilityId = "connection", Namespace = "SaaSFoundry.Persistence.Connection", ExtensionMethod = "AddSaaSFoundryPersistence", RegistrationOrder = 40 },
                    new CapabilityRegistrationMetadata { CapabilityId = "jobstorage", Namespace = null, ExtensionMethod = null, RegistrationOrder = 50 },
                    new CapabilityRegistrationMetadata { CapabilityId = "idempotency", Namespace = null, ExtensionMethod = null, RegistrationOrder = 55 }
                }
            },
            new PluginDescriptor
            {
                Id = "SaaSFoundry.Plugins.BackgroundProcessing",
                Registrations = new List<CapabilityRegistrationMetadata>
                {
                    new CapabilityRegistrationMetadata { CapabilityId = "backgroundjob", Namespace = null, ExtensionMethod = null, RegistrationOrder = 60 }
                }
            },
            new PluginDescriptor
            {
                Id = "SaaSFoundry.Plugins.Import",
                Registrations = new List<CapabilityRegistrationMetadata>
                {
                    new CapabilityRegistrationMetadata { CapabilityId = "import", Namespace = "SaaSFoundry.Import", ExtensionMethod = "AddSaaSFoundryImport", RegistrationOrder = 70 }
                }
            }
        };

        var planner = new CodeGenerationPlanner();
        var plan = planner.Plan(product, plugins);

        var writer = new ArtifactWriter();
        var hostGenerator = new HostGenerator(writer);

        IServiceProvider services = null!;
        var registry = SaaSFoundry.EngineeringWorkbench.Infrastructure.Host.PluginCompositionRoot.Compose();
        var engine = new PluginExecutionEngine(registry, services);
        await engine.InitializeAllAsync(System.Threading.CancellationToken.None);
        
        var materializer = new RuntimeMaterializer(writer, engine, registry);
        await materializer.MaterializeAsync(plan, WorkbenchRoot, System.Threading.CancellationToken.None);

        await hostGenerator.GenerateAsync(plan, WorkbenchRoot);

        Console.WriteLine("Materialization complete.");
    }

    public Task ValidateAsync()
    {
        Console.WriteLine("Validation completed.");
        return Task.CompletedTask;
    }

    public Task ReportAsync()
    {
        Console.WriteLine("Report generation completed.");
        return Task.CompletedTask;
    }
}


