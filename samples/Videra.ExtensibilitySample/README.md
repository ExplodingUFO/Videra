# Videra.ExtensibilitySample

`Videra.ExtensibilitySample` is the primary narrow reference for `MAIN-02`: it shows the public render extensibility flow through Avalonia using `VideraView.Engine`.

## Public APIs Exercised

- `VideraView.Engine`
- `RegisterPassContributor(RenderPassSlot.SolidGeometry, ...)`
- `RegisterFrameHook(RenderFrameHookPoint.FrameEnd, ...)`
- `RenderCapabilities`
- `RenderCapabilities.SupportedFeatureNames`
- `BackendDiagnostics`
- `BackendDiagnostics.LastFrameFeatureNames`
- `BackendDiagnostics.SupportedRenderFeatureNames`
- `LoadModelAsync("Assets/reference-cube.obj")`
- `FrameAll()`

The sample registers exactly one `IRenderPassContributor` for `RenderPassSlot.SolidGeometry`, exactly one `FrameEnd` hook, loads the bundled cube asset, frames the scene, and shows the contributor observation plus capability and diagnostics summaries in the side panel.

Those summaries intentionally surface the public render-feature vocabulary too: `Opaque`, `Transparent`, `Overlay`, `Picking`, and `Screenshot`.

## Run

```bash
dotnet run --project samples/Videra.ExtensibilitySample/Videra.ExtensibilitySample.csproj
```

For build-only verification:

```bash
dotnet build samples/Videra.ExtensibilitySample/Videra.ExtensibilitySample.csproj -c Release
```

## Scope Boundary

This sample is intentionally narrow. `package discovery` and `plugin loading` remain out of scope. The goal is a copyable public-API-only reference for contributor registration, frame hook registration, model loading, framing, and runtime capability/diagnostic queries.

## Main Files

```text
Videra.ExtensibilitySample/
├── Assets/reference-cube.obj
├── Extensibility/RecordingContributor.cs
├── Views/MainWindow.axaml
├── Views/MainWindow.axaml.cs
├── App.axaml
├── App.axaml.cs
└── Program.cs
```

## Related Docs

- [Repository README](../../README.md)
- [Videra.Avalonia](../../src/Videra.Avalonia/README.md)
- [Videra.Core](../../src/Videra.Core/README.md)
