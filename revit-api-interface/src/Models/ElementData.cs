using System.Collections.Generic;

namespace RevitApiInterface.Models;

/// <summary>
/// Flat representation of a Revit element used for extraction and CSV export.
/// </summary>
public sealed class ElementData
{
    public long ElementId { get; init; }
    public string Category { get; init; } = string.Empty;
    public string FamilyName { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public string LevelName { get; init; } = string.Empty;
    public Dictionary<string, string> Parameters { get; init; } = new();
}
