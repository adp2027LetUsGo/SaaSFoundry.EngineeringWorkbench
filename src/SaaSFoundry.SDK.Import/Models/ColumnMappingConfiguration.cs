using System;
using System.Collections.Generic;
using System.Linq;

namespace SaaSFoundry.SDK.Import.Models;

public sealed class ColumnMappingConfiguration
{
    public IReadOnlyList<ColumnMapping> Mappings { get; }

    public ColumnMappingConfiguration(IReadOnlyList<ColumnMapping> mappings)
    {
        Mappings = mappings ?? Array.Empty<ColumnMapping>();
    }

    public void Validate(IEnumerable<string> allowedTargetFields)
    {
        if (Mappings.Count == 0) return;

        var allowedSet = new HashSet<string>(allowedTargetFields, StringComparer.OrdinalIgnoreCase);

        var duplicates = Mappings.GroupBy(m => m.TargetField, StringComparer.OrdinalIgnoreCase)
                                 .Where(g => g.Count() > 1)
                                 .Select(g => g.Key)
                                 .ToList();

        if (duplicates.Any())
        {
            throw new InvalidOperationException($"Duplicate target fields found in mapping configuration: {string.Join(", ", duplicates)}");
        }

        var unknownFields = Mappings.Select(m => m.TargetField)
                                    .Where(t => !allowedSet.Contains(t))
                                    .ToList();

        if (unknownFields.Any())
        {
            throw new InvalidOperationException($"Unknown target fields found in mapping configuration: {string.Join(", ", unknownFields)}");
        }
    }
}
