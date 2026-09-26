using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitApiInterface.Models;

namespace RevitApiInterface.Commands;

/// <summary>
/// Extracts properties (category, family/type, level, instance parameters) from
/// the current selection, or prompts for a pick if nothing is selected.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
public class ExtractElementsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIDocument uiDoc = commandData.Application.ActiveUIDocument;
        Document doc = uiDoc.Document;

        ICollection<ElementId> selectedIds = uiDoc.Selection.GetElementIds();
        if (selectedIds.Count == 0)
        {
            try
            {
                IList<Reference> picked = uiDoc.Selection.PickObjects(
                    ObjectType.Element, "Select elements to extract, then press Finish");
                selectedIds = picked.Select(r => r.ElementId).ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
        }

        var extracted = new List<ElementData>();
        foreach (ElementId id in selectedIds)
        {
            Element element = doc.GetElement(id);
            if (element == null)
            {
                continue;
            }

            extracted.Add(ExtractElement(element));
        }

        TaskDialog.Show(
            "Extract Elements",
            $"Extracted {extracted.Count} element(s).\n\n" +
            string.Join("\n", extracted.Take(10).Select(e => $"#{e.ElementId} — {e.Category}: {e.FamilyName} / {e.TypeName}")));

        return Result.Succeeded;
    }

    internal static ElementData ExtractElement(Element element)
    {
        var data = new ElementData
        {
            ElementId = element.Id.Value,
            Category = element.Category?.Name ?? "Uncategorized",
            FamilyName = (element as FamilyInstance)?.Symbol?.Family?.Name ?? string.Empty,
            TypeName = element.Document.GetElement(element.GetTypeId())?.Name ?? string.Empty,
            LevelName = element.Document.GetElement(element.LevelId)?.Name ?? string.Empty,
        };

        foreach (Parameter parameter in element.Parameters)
        {
            if (!parameter.HasValue || string.IsNullOrEmpty(parameter.Definition?.Name))
            {
                continue;
            }

            data.Parameters[parameter.Definition.Name] = parameter.AsValueString() ?? parameter.AsString() ?? string.Empty;
        }

        return data;
    }
}
