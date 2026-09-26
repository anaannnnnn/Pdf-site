using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitApiInterface.Commands;

/// <summary>
/// Creates a structural framing element (beam) between two picked points on the
/// active level, using the first available structural framing type.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class CreateStructuralElementCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIDocument uiDoc = commandData.Application.ActiveUIDocument;
        Document doc = uiDoc.Document;

        FamilySymbol? framingType = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .OfCategory(BuiltInCategory.OST_StructuralFraming)
            .Cast<FamilySymbol>()
            .FirstOrDefault();

        if (framingType == null)
        {
            message = "No structural framing family types are loaded in this project.";
            return Result.Failed;
        }

        Level? level = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(l => l.Elevation)
            .FirstOrDefault();

        if (level == null)
        {
            message = "No levels found in this project.";
            return Result.Failed;
        }

        XYZ start, end;
        try
        {
            start = uiDoc.Selection.PickPoint("Pick the beam start point");
            end = uiDoc.Selection.PickPoint("Pick the beam end point");
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            return Result.Cancelled;
        }

        using var transaction = new Transaction(doc, "Create Structural Framing");
        transaction.Start();

        if (!framingType.IsActive)
        {
            framingType.Activate();
        }

        Line curve = Line.CreateBound(start, end);
        doc.Create.NewFamilyInstance(curve, framingType, level, StructuralType.Beam);

        transaction.Commit();

        TaskDialog.Show("Create Structural Elements", "Beam created successfully.");
        return Result.Succeeded;
    }
}
