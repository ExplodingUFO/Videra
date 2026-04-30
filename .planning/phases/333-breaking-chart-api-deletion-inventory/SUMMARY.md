# Phase 333 Summary

## Inventory Result

No `VideraChartView` or `Plot.Add.*` implementation exists yet.

Old chart View references are broad and deliberate:

- Source View components:
  - `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView*.cs`
  - `src/Videra.SurfaceCharts.Avalonia/Controls/WaterfallChartView.cs`
  - `src/Videra.SurfaceCharts.Avalonia/Controls/ScatterChartView*.cs`
- Public/package truth:
  - `README.md`
  - `src/Videra.SurfaceCharts.Avalonia/README.md`
  - `src/Videra.SurfaceCharts.Rendering/README.md`
  - `src/Videra.SurfaceCharts.Core/README.md`
  - `eng/public-api-contract.json`
- Proof paths:
  - `samples/Videra.SurfaceCharts.Demo`
  - `smoke/Videra.SurfaceCharts.ConsumerSmoke`
  - `samples/Videra.AvaloniaWorkbenchSample`
- Support/docs:
  - `docs/alpha-feedback.md`
  - `docs/troubleshooting.md`
  - `docs/support-matrix.md`
  - `docs/capability-matrix.md`
  - `docs/package-matrix.md`
  - `docs/zh-CN/*`
- Tests:
  - `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/*SurfaceChartView*`
  - `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/*WaterfallChartView*`
  - `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/*ScatterChartView*`
  - `tests/Videra.Core.Tests/Samples/*SurfaceCharts*`
  - `tests/Videra.Core.Tests/Repository/*SurfaceCharts*`
  - Doctor/support-summary repository tests.

## Classification

- Delete old View components directly:
  - `SurfaceChartView`
  - `WaterfallChartView`
  - `ScatterChartView`
- Migrate public API and usage to `VideraChartView`:
  - package docs
  - demo/smoke XAML and code-behind
  - public API contract
  - support summary chart-control vocabulary
  - repository guardrails
- Retain only non-View helpers when they do not recreate old View abstractions:
  - chart runtime/controller services
  - tile scheduling/cache services
  - camera/projection helpers
  - overlay presenters/coordinators
  - core renderers such as `ScatterRenderer`
  - projection/status DTOs after renaming away from old View-specific vocabulary where needed.

## Minimal Phase 334 API Boundary

```csharp
var view = new VideraChartView();

view.Plot.Add.Surface(matrix);
view.Plot.Add.Waterfall(rows);
view.Plot.Add.Scatter(points);

view.Plot.Axes.X.Label = "X";
view.Plot.Axes.Y.Label = "Y";
view.Plot.Axes.Z.Label = "Z";
view.Plot.Style = PlotStyle.ProfessionalDark();
view.Refresh();
```

Phase 334 should create the foundation, not full final parity. It should keep internals split by chart type and data path, and it should not preserve old View names.

## Handoff

Recommended execution order:

1. Create `VideraChartView`, `Plot3D`, and `Plot3DAddApi`.
2. Build initial `Surface`, `Waterfall`, and `Scatter` series descriptors around existing data contracts.
3. Move only enough control/render dispatch to prove `Plot.Add.*`.
4. Leave old View deletion for Phase 335 after the new foundation compiles.
5. Migrate demo/smoke in Phase 336.
6. Move professional presentation/evidence parity to Phase 337.
7. Add old-View absence guardrails in Phase 338.
