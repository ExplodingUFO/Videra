# Videra.AvaloniaWorkbenchSample

`Videra.AvaloniaWorkbenchSample` is an optional repository sample for a narrow workbench adoption workflow. It composes existing public APIs and does not add a Core dependency, change `VideraView`, or introduce a reusable tooling package.

## Public APIs exercised

- `VideraViewOptions`
- `VideraView.LoadModelAsync(...)`
- `VideraView.ClearScene()`
- `VideraView.AddObject(...)`
- `VideraView.AddInstanceBatch(...)`
- `VideraView.SelectionState`
- `VideraView.Annotations`
- `VideraView.Measurements`
- `VideraDiagnosticsSnapshotFormatter.Format(...)`
- `SceneAuthoring.Create(...)`
- `SceneUploadCoordinator.CreateDeferredObject(...)`
- `ObjModelImporter.Create()`
- `SurfaceChartNumericLabelPresets`

## Workflow

- Load an authored scene built from `SceneAuthoring` and show retained-scene evidence.
- Load the shared `reference-cube.obj` through `LoadModelAsync(...)`.
- Capture diagnostics only on explicit refresh, backend status changes, and support-copy actions.
- Copy a support capture that combines scene evidence, chart precision evidence, and a diagnostics snapshot.
- Demonstrate chart-local numeric precision with SurfaceCharts overlay presets without adding chart semantics to `VideraView`.

## Run

```bash
dotnet run --project samples/Videra.AvaloniaWorkbenchSample/Videra.AvaloniaWorkbenchSample.csproj
```

For build-only verification:

```bash
dotnet build samples/Videra.AvaloniaWorkbenchSample/Videra.AvaloniaWorkbenchSample.csproj -c Debug --no-restore
```

## Scope boundary

This is a sample-first workbench slice. It intentionally avoids a new package, compatibility/fallback layers, Core changes, `VideraView` contract changes, per-frame diagnostics UI updates, and broad workbench features.
