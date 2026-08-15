using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.SDK.Import.DataQuality;
using SaaSFoundry.SDK.Import.Mapping;
using SaaSFoundry.SDK.Import.Models;
using SaaSFoundry.SDK.Import.Parsers;

namespace SaaSFoundry.SDK.Import.Engine;

public sealed class DefaultImportEngine : IImportEngine
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultImportEngine(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private IParser CreateParser(ImportFormat format) => format switch
    {
        ImportFormat.Csv => new CsvParser(),
        ImportFormat.Xlsx => new XlsxParser(),
        _ => throw new NotSupportedException($"Format {format} is not supported.")
    };

    public async Task<ImportSchema> InspectAsync(Stream stream, ImportFormat format, CancellationToken cancellationToken = default)
    {
        var parser = CreateParser(format);
        // We only need the first row to ensure columns are discovered.
        await foreach (var _ in parser.ParseAsync(stream, cancellationToken))
        {
            break;
        }
        return new ImportSchema(parser.Columns);
    }

    public Task<ImportResult<T>> ProcessAsync<T>(Stream stream, ImportFormat format, CancellationToken cancellationToken = default) where T : class
    {
        return ProcessInternalAsync<T>(stream, format, null, cancellationToken);
    }

    public Task<ImportResult<T>> ProcessAsync<T>(Stream stream, ImportFormat format, ColumnMappingConfiguration configuration, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return ProcessInternalAsync<T>(stream, format, configuration, cancellationToken);
    }

    private async Task<ImportResult<T>> ProcessInternalAsync<T>(Stream stream, ImportFormat format, ColumnMappingConfiguration? configuration, CancellationToken cancellationToken) where T : class
    {
        var result = new ImportResult<T>();
        
        var parser = CreateParser(format);

        // Resolve dependencies manually to avoid constructor bloat in DI for open generics
        var mapper = (IImportMapper<T>?)_serviceProvider.GetService(typeof(IImportMapper<T>));
        if (mapper == null) throw new InvalidOperationException($"No IImportMapper registered for type {typeof(T).Name}");

        if (configuration != null)
        {
            if (mapper is IConfigurableImportMapper<T> configurableMapper)
            {
                configuration.Validate(configurableMapper.SupportedTargetFields);
            }
        }

        var rules = (IEnumerable<IDataQualityRule<T>>)_serviceProvider.GetService(typeof(IEnumerable<IDataQualityRule<T>>)) ?? Array.Empty<IDataQualityRule<T>>();

        long rowIndex = 1;
        await foreach (var rawRow in parser.ParseAsync(stream, cancellationToken))
        {
            var mappedRow = ApplyConfiguration(rawRow, configuration);
            var record = new ImportRecord<T>
            {
                RowNumber = rowIndex++,
                Data = mapper.Map(mappedRow)
            };

            foreach (var rule in rules)
            {
                await rule.EvaluateAsync(record);
            }

            // If any diagnostic is Invalid, record becomes Invalid
            // If Warning, record becomes Warning
            // Correctable logic applies if rule flags it
            if (record.Diagnostics.Any(d => d.Category == ImportCategory.Invalid))
                record.Category = ImportCategory.Invalid;
            else if (record.Diagnostics.Any(d => d.Category == ImportCategory.Warning) && record.Category != ImportCategory.Invalid)
                record.Category = ImportCategory.Warning;
            else if (record.Diagnostics.Any(d => d.Category == ImportCategory.Correctable) && record.Category == ImportCategory.Valid)
                record.Category = ImportCategory.Correctable;

            result.Rows.Add(record);
        }

        result.DetectedColumns.AddRange(parser.Columns);
        return result;
    }

    private static IReadOnlyDictionary<string, string> ApplyConfiguration(IReadOnlyDictionary<string, string> rawRow, ColumnMappingConfiguration? config)
    {
        if (config == null || config.Mappings.Count == 0) return rawRow;

        var translated = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        // Preserve all existing columns in case some are mapped dynamically and some are still statically evaluated
        foreach (var kvp in rawRow)
        {
            translated[kvp.Key] = kvp.Value;
        }

        foreach (var mapping in config.Mappings)
        {
            if (rawRow.TryGetValue(mapping.SourceColumn, out var val))
            {
                translated[mapping.TargetField] = val;
            }
        }

        return translated;
    }
}
