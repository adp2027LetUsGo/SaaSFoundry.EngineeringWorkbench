using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class TableRenderer
{
    private readonly IConsoleRenderer _console;

    public TableRenderer(IConsoleRenderer console)
    {
        _console = console;
    }

    public void DrawTable(string title, string[] headers, List<string[]> rows)
    {
        if (rows.Count == 0)
        {
            _console.WriteLine(title);
            _console.WriteLine("┌───────────────────────────┐");
            _console.WriteLine("│ (No data to display)      │");
            _console.WriteLine("└───────────────────────────┘");
            _console.WriteLine();
            return;
        }

        var colWidths = new int[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            colWidths[i] = headers[i].Length;
        }

        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] != null && row[i].Length > colWidths[i])
                {
                    colWidths[i] = row[i].Length;
                }
            }
        }

        _console.WriteLine(title);

        var topBorder = new StringBuilder("┌");
        var headerRow = new StringBuilder("│");
        var sepBorder = new StringBuilder("├");
        var botBorder = new StringBuilder("└");

        for (int i = 0; i < headers.Length; i++)
        {
            var width = colWidths[i] + 2;
            topBorder.Append(new string('─', width));
            sepBorder.Append(new string('─', width));
            botBorder.Append(new string('─', width));

            headerRow.Append(" " + headers[i].PadRight(width - 1) + "│");

            if (i < headers.Length - 1)
            {
                topBorder.Append("┬");
                sepBorder.Append("┼");
                botBorder.Append("┴");
            }
        }
        
        topBorder.Append("┐");
        sepBorder.Append("┤");
        botBorder.Append("┘");

        _console.WriteLine(topBorder.ToString());
        _console.WriteLine(headerRow.ToString());
        _console.WriteLine(sepBorder.ToString());

        foreach (var row in rows)
        {
            var rowStr = new StringBuilder("│");
            for (int i = 0; i < row.Length; i++)
            {
                var width = colWidths[i] + 2;
                var val = row[i] ?? "";
                rowStr.Append(" " + val.PadRight(width - 1) + "│");
            }
            _console.WriteLine(rowStr.ToString());
        }

        _console.WriteLine(botBorder.ToString());
        _console.WriteLine();
    }
}
