# Videra.InteractionSample

`Videra.InteractionSample` is the focused public interaction reference for controlled selection and annotations in Avalonia.

## Public contract exercised

- The host owns `SelectionState` and annotation state
- Built-in interaction modes: `Navigate`, `Select`, `Annotate`
- Selection is `object-level`
- The host applies `SelectionRequested` and `AnnotationRequested`
- Annotations use `VideraNodeAnnotation` and `VideraWorldPointAnnotation`
- Overlay responsibilities are split between `3D highlight/render state` and `2D label/feedback rendering`

The sample keeps the interaction flow on public APIs only:

1. It loads a narrow two-object scene through `LoadModelsAsync(...)`.
2. The window sets `View3D.SelectionState = _selectionState;` and `View3D.Annotations = _annotations;`.
3. The window switches `View3D.InteractionMode` between `Navigate`, `Select`, and `Annotate`.
4. `SelectionRequested` updates host-owned `SelectionState`.
5. `AnnotationRequested` appends either a `VideraNodeAnnotation` or a `VideraWorldPointAnnotation`.

`host owns` is the key rule: the control reports intent, while the sample window decides how selection and annotation state change.

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
