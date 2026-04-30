# Phase 187 Summary: Install Truth, Size Evidence, and Guardrails

## Outcome

The tightened Avalonia install boundary is now taught and enforced consistently.

What changed:

- canonical install docs now describe one viewer-only path and one importer-enabled path
- `Videra.Avalonia` package docs now require explicit `Videra.Import.*` installation plus `VideraViewOptions.ModelImporter` for file loading
- `Validate-Packages.ps1` now enforces the slimmer default Avalonia dependency shape
- repository guardrails now expect `Videra.Avalonia` to stay core-only at the project-reference layer
- `Videra.Avalonia` package-size budgets were tightened to reflect the slimmer default package surface

With this phase complete, `v2.13` is ready for local audit and closeout.
