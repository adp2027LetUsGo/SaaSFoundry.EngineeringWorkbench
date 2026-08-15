using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Builder.Models;

namespace SaaSFoundry.EngineeringWorkbench.Builder.Services;

public sealed class CodeGenerationPlanner
{
    public CodeGenerationPlan Plan(ProductDefinition product, IEnumerable<PluginDescriptor> availablePlugins)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        if (availablePlugins == null) throw new ArgumentNullException(nameof(availablePlugins));

        var plan = new CodeGenerationPlan
        {
            ProductId = product.ProductId,
            Product = product
        };

        foreach (var cell in product.Cells)
        {
            var cellPlan = new CellGenerationPlan
            {
                CellId = cell.CellId,
                TargetPath = cell.TargetPath
            };

            var registrations = new List<CapabilityRegistrationMetadata>();

            foreach (var requestedCapability in cell.Capabilities)
            {
                var foundRegistration = false;
                
                foreach (var descriptor in availablePlugins)
                {
                    if (descriptor.Registrations != null)
                    {
                        var match = descriptor.Registrations.FirstOrDefault(r => r.CapabilityId == requestedCapability);
                        if (match != null)
                        {
                            // Namespace and ExtensionMethod are now optional because some capabilities do not generate extension methods.

                            
                            if (registrations.Any(r => r.CapabilityId == requestedCapability))
                            {
                                throw new InvalidOperationException($"Duplicate registration found for capability '{requestedCapability}'.");
                            }

                            registrations.Add(match);
                            foundRegistration = true;
                            break;
                        }
                    }
                }

                if (!foundRegistration)
                {
                    throw new InvalidOperationException($"Capability '{requestedCapability}' referenced by Cell '{cell.CellId}' is missing registration metadata in any registered plugin.");
                }
            }

            // Order deterministically
            cellPlan.Registrations = registrations.OrderBy(r => r.RegistrationOrder).ThenBy(r => r.CapabilityId).ToList();
            plan.Cells.Add(cellPlan);
        }

        return plan;
    }
}

