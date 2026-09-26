using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitApiInterface.Models;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace RevitApiInterface.Commands;

/// <summary>
/// Extracts the current selection (or the whole model if nothing is selected)
/// and writes it to a CSV file for use in downstream analysis software.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
public class ExportElementsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIDocument uiDoc = commandData.Application.ActiveUIDocument;
        Document doc = uiDoc.Document;

        ICollection<ElementId> selectedIds = uiDoc.Selection.GetElementIds();
        IEnumerable<Element> sourceElements = selectedIds.Count > 0
            ? selectedIds.Select(doc.GetElement)
            : new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .Where(e => e.Category != null);

        List<ElementData> extracted = sourceElements
            .Where(e => e != null)
            .Select(ExtractElementsCommand.ExtractElement)
            .ToList();

        if (extracted.Count == 0)
        {
            TaskDialog.Show("Export Elements", "Nothing to export.");
            return Result.Succeeded;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = "revit-element-export.csv",
        };

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return Result.Cancelled;
        }

        WriteCsv(dialog.FileName, extracted);

        TaskDialog.Show("Export Elements", $"Exported {extracted.Count} element(s) to {dialog.FileName}.");
        return Result.Succeeded;
    }

    private static void WriteCsv(string path, IReadOnlyCollection<ElementData> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("ElementId,Category,FamilyName,TypeName,LevelName");

        foreach (ElementData row in rows)
        {
            builder.AppendLine(string.Join(",",
                row.ElementId.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(row.Category),
                EscapeCsv(row.FamilyName),
                EscapeCsv(row.TypeName),
                EscapeCsv(row.LevelName)));
        }

        File.WriteAllText(path, builder.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
