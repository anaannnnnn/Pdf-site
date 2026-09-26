using System.Reflection;
using Autodesk.Revit.UI;

namespace RevitApiInterface;

/// <summary>
/// Entry point registered in RevitApiInterface.addin. Adds a ribbon panel with
/// buttons for each command implemented under Commands/.
/// </summary>
public class RevitApiInterfaceApp : IExternalApplication
{
    private const string TabName = "Revit API Interface";
    private const string PanelName = "Automation";

    public Result OnStartup(UIControlledApplication application)
    {
        application.CreateRibbonTab(TabName);
        RibbonPanel panel = application.CreateRibbonPanel(TabName, PanelName);

        string assemblyPath = Assembly.GetExecutingAssembly().Location;

        AddButton(panel, assemblyPath,
            "ExtractElements", "Extract\nElements",
            "RevitApiInterface.Commands.ExtractElementsCommand");

        AddButton(panel, assemblyPath,
            "CreateStructural", "Create\nStructural",
            "RevitApiInterface.Commands.CreateStructuralElementCommand");

        AddButton(panel, assemblyPath,
            "GenerateSchedule", "Generate\nSchedule",
            "RevitApiInterface.Commands.GenerateScheduleCommand");

        AddButton(panel, assemblyPath,
            "ExportElements", "Export\nElements",
            "RevitApiInterface.Commands.ExportElementsCommand");

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

    private static void AddButton(RibbonPanel panel, string assemblyPath, string internalName, string label, string className)
    {
        var data = new PushButtonData(internalName, label, assemblyPath, className);
        panel.AddItem(data);
    }
}
