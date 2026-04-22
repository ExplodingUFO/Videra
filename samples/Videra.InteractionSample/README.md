# Videra.InteractionSample

`Videra.InteractionSample` is the focused public interaction and inspection reference for controlled selection, annotations, measurements, clipping, snapshot export, and replayable inspection bundles in Avalonia.

## Public contract exercised

- The host owns `SelectionState` and annotation state
- The host owns `Annotations`
- Built-in interaction modes: `Navigate`, `Select`, `Annotate`, `Measure`
- Selection is `object-level`
- The host applies `SelectionRequested` and `AnnotationRequested`
- `AnnotationRequested` resolves to object anchors and world-point anchors
- Measurements stay on the public `Measurements` surface and are created through `Measure`
- Clipping stays on the public `ClippingPlanes` surface
- Inspection persistence/export uses `CaptureInspectionState()`, `ApplyInspectionState(...)`, `ExportSnapshotAsync(...)`, and `VideraInspectionBundleService`
- Annotations use `VideraNodeAnnotation` and `VideraWorldPointAnnotation`
- Overlay responsibilities are split between `3D highlight/render state` and `2D label/feedback rendering`

The sample keeps the interaction flow on public APIs only:

1. It imports `Assets/reference-cube.obj` twice through `ObjModelImporter.Import(...)`, wraps each imported asset with `SceneUploadCoordinator.CreateDeferredObject(...)`, and replaces the active scene through `View3D.ReplaceScene(...)`.
2. The window sets `View3D.SelectionState = _selectionState;` and `View3D.Annotations = _annotations;`.
3. The window switches `View3D.InteractionMode` between `Navigate`, `Select`, `Annotate`, and `Measure`.
4. `SelectionRequested` updates host-owned `SelectionState`.
5. `AnnotationRequested` appends either a `VideraNodeAnnotation` for an object anchor or a `VideraWorldPointAnnotation` for a world-point anchor.
6. `Measure` writes lightweight distance probes to `View3D.Measurements`.
7. The inspection panel toggles `ClippingPlanes`, saves/restores one typed inspection session through `CaptureInspectionState()` / `ApplyInspectionState(...)`, calls `ExportSnapshotAsync(...)`, and round-trips replayable bundles through `VideraInspectionBundleService` on top of that same session truth.

`host owns` is still the key rule for selection and annotations: the control reports intent, while the sample window decides how `SelectionState` and `Annotations` change. Saved inspection state now includes those host-owned annotations alongside camera, measurements, clipping, and snap mode, while replayable bundle export/import stays on the same public inspection surface without reaching into internal runtime seams.

## Run

```bash
dotnet run --project samples/Videra.InteractionSample/Videra.InteractionSample.csproj
```

For build-only verification:

```bash
dotnet build samples/Videra.InteractionSample/Videra.InteractionSample.csproj -c Release
```

## Scope boundary

This sample demonstrates the public interaction flow, not internal seams. It does not reach into `SelectionOverlayRenderState`, `AnnotationOverlayRenderState`, `VideraViewSessionBridge`, or other internal overlay plumbing.

## Main files

```text
Videra.InteractionSample/
├── Assets/reference-cube.obj
├── Views/MainWindow.axaml
├── Views/MainWindow.axaml.cs
├── App.axaml
├── App.axaml.cs
└── Program.cs
```

## Related docs

- [Repository README](../../README.md)
- [Videra.Avalonia](../../src/Videra.Avalonia/README.md)
- [Chinese entry](../../docs/zh-CN/README.md)
