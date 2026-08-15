using System;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public class CliRenderer : IConsoleRenderer
{
    public const string IconSuccess = "✓";
    public const string IconWarning = "⚠";
    public const string IconError = "✗";
    public const string IconInfo = "ℹ";
    public const string IconRunning = "▶";

    public void WriteLine(string text = "")
    {
        Console.WriteLine(text);
    }

    public void Write(string text)
    {
        Console.Write(text);
    }

    public void DrawHeader(string title)
    {
        WriteLine("===========================================================");
        WriteLine($"        {title}");
        WriteLine("===========================================================");
        WriteLine();
    }

    public void DrawSection(string title)
    {
        WriteLine(title);
        WriteLine("-----------------------------------------------------------");
    }

    public void DrawWarning(string text)
    {
        WriteLine($"{IconWarning} {text}");
    }

    public void DrawError(string text)
    {
        WriteLine($"{IconError} {text}");
    }

    public void DrawSuccess(string text)
    {
        WriteLine($"{IconSuccess} {text}");
    }
}
