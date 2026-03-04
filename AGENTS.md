# Repository Guidelines

## Project Structure & Module Organization
- `SCSC_Marcas.sln` is the main Visual Studio solution (single app project: `SCSC/`).
- `SCSC/Formularios/` contains WinForms screens (`Frm*.vb`) and user controls.
- `SCSC/Clases/` holds shared logic (DB access, encryption, globals).
- `SCSC/Reportes/` contains Crystal Reports forms, parameters, and `.rpt` files.
- `SCSC/Seguridad/` includes login/auth UI.
- `SCSC/My Project/` stores assembly metadata, resources, and app settings.
- `packages/` contains restored NuGet dependencies (`Bunifu.UI.WinForms`, `GCDesign`).
- `Utilitarios/` and spreadsheet folders are support assets/import inputs; avoid changing unless required.

## Build, Test, and Development Commands
- Restore packages:
```bash
nuget restore SCSC_Marcas.sln
```
- Build Debug:
```bash
msbuild SCSC_Marcas.sln /p:Configuration=Debug /p:Platform="Any CPU"
```
- Build Release x64:
```bash
msbuild SCSC_Marcas.sln /p:Configuration=Release /p:Platform=x64
```
- Run locally: open `SCSC_Marcas.sln` in Visual Studio 2019+ and start `SCSC_Marcas`.

## Coding Style & Naming Conventions
- Language: VB.NET (.NET Framework 4.6.1), Windows Forms.
- Keep existing style: 4-space indentation, `Option Explicit On` where practical, PascalCase for types/methods.
- Form naming follows current patterns: `Frm*` for windows, `Control*` for reusable controls.
- Do not hand-edit `*.Designer.vb` unless the designer is unavailable; prefer Visual Studio designer changes.

## Testing Guidelines
- No automated test project is currently present.
- For each change, run a manual smoke test of affected flows (login, mark registration, report generation, import screens).
- For report or UI changes, validate both visual behavior and database interactions.
- Document manual verification steps in the PR description.

## Commit & Pull Request Guidelines
- Existing history uses short, task-focused Spanish summaries (example: `Refactorizacion 2026`).
- Write commits in imperative mood, scoped to one logical change.
- PRs should include:
  - Purpose and impacted modules (e.g., `SCSC/Formularios/FrmRutas.vb`).
  - Setup/data needed to reproduce.
  - Screenshots for UI/report changes.
  - Manual test evidence and any known limitations.

## Security & Configuration Tips
- Connection and app settings are read from `SCSC/app.config`; never commit real credentials.
- DigitalPersona and Crystal Reports dependencies are environment-sensitive; confirm local runtime versions before merging.
