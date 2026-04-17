# Videra.MinimalSample

`Videra.MinimalSample` is the narrow first-scene reference for the public alpha viewer path.

## Public APIs exercised

- `VideraViewOptions`
- `LoadModelAsync("Assets/reference-cube.obj")`
- `FrameAll()`
- `ResetCamera()`
- `BackendDiagnostics`

The sample keeps the default flow intentionally short:

1. Configure `View3D.Options` with `AllowSoftwareFallback = true`.
2. Wait for `BackendReady` / `BackendDiagnostics.IsReady`.
3. Call `LoadModelAsync("Assets/reference-cube.obj")`.
4. Call `FrameAll()`.
5. Read `BackendDiagnostics`.

## Run

```bash
dotnet run --project samples/Videra.MinimalSample/Videra.MinimalSample.csproj
```

For build-only verification:

```bash
dotnet build samples/Videra.MinimalSample/Videra.MinimalSample.csproj -c Release
```

## Scope boundary

This sample is intentionally limited to the alpha happy path. It does not call `VideraView.Engine`, does not register frame hooks, and does not depend on internal runtime seams.

## Main files

```text
Videra.MinimalSample/
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
- [Videra.ExtensibilitySample](../Videra.ExtensibilitySample/README.md)
