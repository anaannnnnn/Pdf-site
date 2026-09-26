using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitApiInterface.Commands;

/// <summary>
/// Creates a basic schedule view for structural framing elements, adding a
/// handful of commonly useful fields.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class GenerateScheduleCommand : IExternalCommand
{
    private const BuiltInCategory Category = BuiltInCategory.OST_StructuralFraming;

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        Document doc = commandData.Application.ActiveUIDocument.Document;

        using var transaction = new Transaction(doc, "Generate Schedule");
        transaction.Start();

        ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, new ElementId(Category));

        SchedulableField[] fields = schedule.Definition.GetSchedulableFields().ToArray();
        AddFieldIfPresent(schedule, fields, "Family");
        AddFieldIfPresent(schedule, fields, "Type");
        AddFieldIfPresent(schedule, fields, "Level");
        AddFieldIfPresent(schedule, fields, "Length");
        AddFieldIfPresent(schedule, fields, "Comments");

        transaction.Commit();

        TaskDialog.Show("Generate Schedule", $"Created schedule '{schedule.Name}'.");
        return Result.Succeeded;
    }

    private static void AddFieldIfPresent(ViewSchedule schedule, IEnumerable<SchedulableField> fields, string fieldName)
    {
        SchedulableField? field = fields.FirstOrDefault(f => f.GetName(schedule.Document) == fieldName);
        if (field != null)
        {
            schedule.Definition.AddField(field);
        }
    }
}
