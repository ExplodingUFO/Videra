# Videra.ExtensibilitySample

`Videra.ExtensibilitySample` is the primary narrow reference for `MAIN-02`: it shows the public render extensibility flow through Avalonia using `VideraView.Engine`.

## Public APIs Exercised

- `VideraView.Engine`
- `RegisterPassContributor(RenderPassSlot.SolidGeometry, ...)`
- `RegisterFrameHook(RenderFrameHookPoint.FrameEnd, ...)`
- `RenderCapabilities`
- `RenderCapabilities.SupportedFeatureNames`
- `RenderCapabilities.SupportsShaderCreation`
- `RenderCapabilities.SupportsResourceSetCreation`
- `RenderCapabilities.SupportsResourceSetBinding`
- `BackendDiagnostics`
- `BackendDiagnostics.LastFrameFeatureNames`
- `BackendDiagnostics.LastFrameObjectCount`
- `BackendDiagnostics.LastFrameOpaqueObjectCount`
- `BackendDiagnostics.LastFrameTransparentObjectCount`
- `BackendDiagnostics.SupportedRenderFeatureNames`
- `BackendDiagnostics.SupportsShaderCreation`
- `BackendDiagnostics.SupportsResourceSetCreation`
- `BackendDiagnostics.SupportsResourceSetBinding`
- `LoadModelAsync("Assets/reference-cube.obj")`
- `FrameAll()`

The sample registers exactly one `IRenderPassContributor` for `RenderPassSlot.SolidGeometry`, exactly one `FrameEnd` hook, loads the bundled cube asset, frames the scene, and shows the contributor observation plus capability and diagnostics summaries in the side panel. The diagnostics summary includes backend-neutral `LastFrameObjectCount`, `LastFrameOpaqueObjectCount`, and `LastFrameTransparentObjectCount` fields plus the advanced shader/resource-set support flags.

Those summaries intentionally surface the public render-feature vocabulary too: `Opaque`, `Transparent`, `Overlay`, `Picking`, and `Screenshot`, where `Transparent` means alpha mask rendering plus deterministic alpha blend ordering for per-primitive carried alpha sources. The last-frame counts are backend-neutral scene diagnostics, not draw-call metrics.

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
