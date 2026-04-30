# Phase 402 Plan: Interaction and Code Experience Polish

## Scope

- Add typed `Plot3DSeries` handles for bar and contour series.
- Add a bounded command discovery surface on `SurfaceChartInteractionProfile`.
- Keep implementation local to plot and interaction controls.

## Success Criteria

- `Plot.Add.Bar(...)` returns `BarPlot3DSeries` while remaining assignable to `Plot3DSeries` and `IPlottable3D`.
- `Plot.Add.Contour(...)` returns `ContourPlot3DSeries` while remaining assignable to `Plot3DSeries` and `IPlottable3D`.
- `SurfaceChartInteractionProfile.EnabledCommands` lists only commands enabled by the existing profile switches.
- Focused Avalonia integration tests cover the API behavior.

## Non-Goals

- No command framework.
- No mouse remapping.
- No compatibility wrappers or old chart controls.
- No demo, cookbook, README, roadmap, bead, or broad documentation edits.

## Validation

- Run focused integration tests for plot API and keyboard/toolbar command behavior.
- Run `git diff --check`.
- Run `git status --short`.
