using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using SaaSFoundry.SDK.Import.Parsers;

namespace SaaSFoundry.SDK.Import.Parsers;

public sealed class CsvParser : IParser
{
    public List<string> Columns { get; } = new();

    public async IAsyncEnumerable<IReadOnlyDictionary<string, string>> ParseAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
            yield break;
        
        Columns.AddRange(ParseLine(headerLine));

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
                continue;
                
            var values = ParseLine(line);
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < Columns.Count; i++)
            {
                dict[Columns[i]] = i < values.Count ? values[i] : string.Empty;
            }
            yield return dict;
        }
    }

    private static List<string> ParseLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(line[i]);
            }
        }
        result.Add(current.ToString());
        return result;
    }
}
