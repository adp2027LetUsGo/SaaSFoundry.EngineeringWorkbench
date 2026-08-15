using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using SaaSFoundry.SDK.Import.Parsers;

namespace SaaSFoundry.SDK.Import.Parsers;

public sealed class XlsxParser : IParser
{
    public List<string> Columns { get; } = new();

    public async IAsyncEnumerable<IReadOnlyDictionary<string, string>> ParseAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // OpenXml reads from stream synchronously, so we run the extraction in a Task to yield async
        var rows = await Task.Run(() => ExtractRows(stream), cancellationToken);
        
        if (rows.Count == 0) yield break;

        Columns.AddRange(rows[0]);

        for (int r = 1; r < rows.Count; r++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < Columns.Count; i++)
            {
                dict[Columns[i]] = i < rows[r].Count ? rows[r][i] : string.Empty;
            }
            yield return dict;
        }
    }

    private List<List<string>> ExtractRows(Stream stream)
    {
        var result = new List<List<string>>();
        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart;
        if (workbookPart == null) return result;

        var sheet = workbookPart.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault();
        if (sheet == null || sheet.Id == null) return result;

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
        if (sheetData == null) return result;

        var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

        foreach (var row in sheetData.Elements<Row>())
        {
            var rowData = new List<string>();
            foreach (var cell in row.Elements<Cell>())
            {
                rowData.Add(GetCellValue(cell, sharedStringTable));
            }
            result.Add(rowData);
        }
        return result;
    }

    private static string GetCellValue(Cell cell, SharedStringTable? sst)
    {
        var value = cell.CellValue?.Text ?? string.Empty;
        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && sst != null)
        {
            if (int.TryParse(value, out int id))
            {
                return sst.ChildElements[id].InnerText;
            }
        }
        return value;
    }
}
