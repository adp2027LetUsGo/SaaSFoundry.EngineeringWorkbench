using System;

namespace SaaSFoundry.EngineeringWorkbench.UI.Presentation;

public interface IConsoleRenderer
{
    void WriteLine(string text = "");
    void Write(string text);
    void DrawHeader(string title);
    void DrawSection(string title);
    void DrawWarning(string text);
    void DrawError(string text);
    void DrawSuccess(string text);
}
