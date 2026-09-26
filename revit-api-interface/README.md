# Revit API Interface — Add-in Scaffold

A starter Autodesk Revit add-in implementing the capabilities described in
the [`revit-api-interface`](../.claude/skills/revit-api-interface/SKILL.md)
skill: element extraction, structural element creation, schedule generation,
and CSV export.

## What this is (and isn't)

This is source code for a Revit add-in — a .NET assembly that Revit loads
at runtime via a `.addin` manifest. **It cannot run outside Revit itself.**
There is no way to generate an actual `.rvt` project file, or exercise the
Revit API, without a licensed copy of Autodesk Revit running on Windows.

This scaffold has **not been built or tested** — the sandbox this was
written in has no Revit installation, no `RevitAPI.dll`/`RevitAPIUI.dll`,
no .NET SDK, and no Windows. Treat it as a reviewed starting point, not a
verified build. Compile it in Visual Studio on a machine with Revit
installed before relying on it.

## Layout

```
revit-api-interface/
├── RevitApiInterface.addin      # manifest Revit reads on startup
└── src/
    ├── RevitApiInterface.csproj
    ├── RevitApiInterfaceApp.cs  # IExternalApplication, builds the ribbon
    ├── Commands/
    │   ├── ExtractElementsCommand.cs
    │   ├── CreateStructuralElementCommand.cs
    │   ├── GenerateScheduleCommand.cs
    │   └── ExportElementsCommand.cs
    └── Models/
        └── ElementData.cs
```

## Prerequisites

- Autodesk Revit installed (2024 targeted by default; see below for other
  versions)
- Visual Studio 2022 (or `dotnet` CLI with the .NET Framework 4.8 targeting
  pack) on Windows
- `RevitAPI.dll` and `RevitAPIUI.dll` from your Revit install directory —
  these ship with Revit and are **not** included or redistributed here

## Build

```powershell
dotnet build src/RevitApiInterface.csproj -p:RevitInstallDir="C:\Program Files\Autodesk\Revit 2024"
```

If you omit `RevitInstallDir`, the project falls back to
`C:\Program Files\Autodesk\Revit $(RevitVersion)` (`RevitVersion` defaults
to `2024` in the `.csproj`).

### Targeting a different Revit version

- **Revit 2022–2024**: no changes needed; these all target `.NET Framework
  4.8` (`net48`).
- **Revit 2025+**: Autodesk moved to `.NET 8`. Change `TargetFramework` to
  `net8.0-windows` in `RevitApiInterface.csproj` and update `RevitVersion`.

## Install into Revit

1. Build the project (produces `RevitApiInterface.dll` and copies
   `RevitApiInterface.addin` next to it).
2. Copy both files to:
   `%AppData%\Autodesk\Revit\Addins\<version>\`
3. Launch Revit. A new **Revit API Interface** ribbon tab appears with four
   commands: Extract Elements, Create Structural, Generate Schedule, Export
   Elements.

## Commands

| Command | What it does |
|---|---|
| Extract Elements | Reads category, family/type, level, and parameters from the current selection (or prompts you to pick) and shows a summary |
| Create Structural | Creates a structural framing (beam) element between two picked points, using the first loaded framing type and lowest level |
| Generate Schedule | Creates a `ViewSchedule` for structural framing with Family/Type/Level/Length/Comments fields |
| Export Elements | Extracts the selection (or the whole model) and writes it to a CSV file for use in external analysis software |

These are intentionally minimal reference implementations meant to be
extended — see the skill's "Capabilities" list
(`../.claude/skills/revit-api-interface/SKILL.md`) for the fuller feature
set (rebar detailing automation, family parameter management, view/sheet
automation, etc.) that isn't yet implemented here.
